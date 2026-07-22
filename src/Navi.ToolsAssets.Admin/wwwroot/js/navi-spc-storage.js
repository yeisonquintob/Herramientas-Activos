(function () {
    "use strict";

    /*
     * NAVI
     *
     * Los resultados grandes de localStorage no se retornan
     * completos a Blazor. Se mantienen temporalmente dentro
     * del navegador y se entregan en fragmentos pequeños.
     */

    const readBuffers = new Map();

    function createToken() {
        if (
            window.crypto &&
            typeof window.crypto.randomUUID === "function"
        ) {
            return window.crypto.randomUUID();
        }

        return (
            Date.now().toString(36) +
            "-" +
            Math.random().toString(36).slice(2)
        );
    }

    function normalizeChunkSize(chunkSize) {
        const numericValue = Number(chunkSize);

        if (
            !Number.isFinite(numericValue) ||
            numericValue < 1000
        ) {
            return 12000;
        }

        return Math.floor(numericValue);
    }

    function sanitizeValue(value, state) {
        if (Array.isArray(value)) {
            for (
                let index = 0;
                index < value.length;
                index += 1
            ) {
                value[index] = sanitizeValue(
                    value[index],
                    state
                );
            }

            return value;
        }

        if (
            value !== null &&
            typeof value === "object"
        ) {
            const evidenceIdProperty =
                Object.keys(value).find(
                    propertyName =>
                        propertyName.toLowerCase() ===
                        "evidenceid"
                );

            const hasPersistentEvidence =
                typeof evidenceIdProperty === "string" &&
                typeof value[evidenceIdProperty] === "string" &&
                value[evidenceIdProperty].trim().length > 0;

            for (
                const propertyName
                of Object.keys(value)
            ) {
                const normalizedName =
                    propertyName.toLowerCase();

                const currentValue =
                    value[propertyName];

                const isDataImage =
                    typeof currentValue === "string" &&
                    currentValue.toLowerCase().startsWith(
                        "data:image/"
                    );

                const mustRemovePreview =
                    normalizedName === "imagepreview" ||
                    (
                        normalizedName === "evidencepreview" &&
                        hasPersistentEvidence
                    );

                if (mustRemovePreview && isDataImage) {
                    state.removedImagePreviews += 1;

                    /*
                     * EvidenceId y el nombre permanecen. Solo
                     * se retira el Base64 que ya está en MinIO.
                     */
                    value[propertyName] = "";

                    continue;
                }

                value[propertyName] =
                    sanitizeValue(
                        value[propertyName],
                        state
                    );
            }
        }

        return value;
    }

    function parseAndSanitize(rawValue) {
        const parsedValue =
            JSON.parse(rawValue);

        const state = {
            removedImagePreviews: 0
        };

        const sanitizedValue =
            sanitizeValue(
                parsedValue,
                state
            );

        return {
            json: JSON.stringify(
                sanitizedValue
            ),
            removedImagePreviews:
                state.removedImagePreviews
        };
    }

    function saveCleanedValue(key, cleanedJson) {
        try {
            localStorage.setItem(
                key,
                cleanedJson
            );

            return;
        } catch {
            /*
             * Si el valor anterior ocupaba toda la cuota,
             * primero se retira y después se almacena la
             * versión depurada.
             */
        }

        localStorage.removeItem(key);

        localStorage.setItem(
            key,
            cleanedJson
        );
    }

    window.naviSpcStorage = {
        prepareRead: function (
            key,
            requestedChunkSize
        ) {
            const chunkSize =
                normalizeChunkSize(
                    requestedChunkSize
                );

            let rawValue;

            try {
                rawValue =
                    localStorage.getItem(key);
            } catch (error) {
                return {
                    token: null,
                    chunkCount: 0,
                    originalLength: 0,
                    finalLength: 0,
                    removedImagePreviews: 0,
                    reset: true,
                    message:
                        "No fue posible acceder al " +
                        "almacenamiento local: " +
                        String(
                            error &&
                            error.message
                                ? error.message
                                : error
                        )
                };
            }

            if (
                rawValue === null ||
                rawValue.trim() === ""
            ) {
                return {
                    token: null,
                    chunkCount: 0,
                    originalLength: 0,
                    finalLength: 0,
                    removedImagePreviews: 0,
                    reset: false,
                    message: null
                };
            }

            const originalLength =
                rawValue.length;

            try {
                const result =
                    parseAndSanitize(
                        rawValue
                    );

                const cleanedJson =
                    result.json;

                /*
                 * Se reemplaza el valor anterior para que
                 * futuras aperturas ya no procesen Base64.
                 */
                if (
                    cleanedJson !== rawValue
                ) {
                    saveCleanedValue(
                        key,
                        cleanedJson
                    );
                }

                const token =
                    createToken();

                readBuffers.set(
                    token,
                    cleanedJson
                );

                return {
                    token: token,
                    chunkCount: Math.ceil(
                        cleanedJson.length /
                        chunkSize
                    ),
                    originalLength:
                        originalLength,
                    finalLength:
                        cleanedJson.length,
                    removedImagePreviews:
                        result.removedImagePreviews,
                    reset: false,
                    message: null
                };
            } catch (error) {
                /*
                 * Un JSON dañado no debe cerrar el circuito.
                 * Se elimina únicamente la clave dañada.
                 */
                try {
                    localStorage.removeItem(
                        key
                    );
                } catch {
                    // No es necesario propagar el error.
                }

                return {
                    token: null,
                    chunkCount: 0,
                    originalLength:
                        originalLength,
                    finalLength: 0,
                    removedImagePreviews: 0,
                    reset: true,
                    message:
                        "El almacenamiento local de esta " +
                        "pantalla estaba dañado y fue " +
                        "reiniciado de forma segura."
                };
            }
        },

        readChunk: function (
            token,
            index,
            requestedChunkSize
        ) {
            const chunkSize =
                normalizeChunkSize(
                    requestedChunkSize
                );

            const value =
                readBuffers.get(token);

            if (
                typeof value !== "string"
            ) {
                return "";
            }

            const safeIndex =
                Math.max(
                    0,
                    Number(index) || 0
                );

            const start =
                safeIndex * chunkSize;

            return value.slice(
                start,
                start + chunkSize
            );
        },

        releaseRead: function (token) {
            if (
                typeof token === "string" &&
                token.length > 0
            ) {
                readBuffers.delete(token);
            }
        },

        write: function (
            key,
            json
        ) {
            try {
                const originalValue =
                    typeof json === "string"
                        ? json
                        : "[]";

                const result =
                    parseAndSanitize(
                        originalValue
                    );

                saveCleanedValue(
                    key,
                    result.json
                );

                return {
                    saved: true,
                    finalLength:
                        result.json.length,
                    removedImagePreviews:
                        result.removedImagePreviews,
                    message: null
                };
            } catch (error) {
                return {
                    saved: false,
                    finalLength: 0,
                    removedImagePreviews: 0,
                    message:
                        "No fue posible guardar el " +
                        "borrador local: " +
                        String(
                            error &&
                            error.message
                                ? error.message
                                : error
                        )
                };
            }
        },

        inspect: function (key) {
            const value =
                localStorage.getItem(key);

            return {
                exists: value !== null,
                length:
                    value === null
                        ? 0
                        : value.length,
                imagePreviewCount:
                    value === null
                        ? 0
                        : (
                            value.match(
                                /data:image\//gi
                            ) || []
                        ).length
            };
        }
    };
})();
