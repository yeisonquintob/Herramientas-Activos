(function () {
    "use strict";

    const initializationKey =
        "__naviAdminUiPreferencesInitialized";

    /*
     * Evita registrar dos veces los eventos cuando el recurso
     * sea evaluado nuevamente por navegación mejorada.
     */
    if (window[initializationKey]) {
        if (
            window.naviUiPreferences &&
            typeof window.naviUiPreferences.sync === "function"
        ) {
            window.naviUiPreferences.sync();
        }

        return;
    }

    window[initializationKey] = true;

    const storageKey =
        "navi-admin-ui-preferences-v1";

    const defaults = {
        fontScale: "normal",
        colorTheme: "normal"
    };

    const validFonts = new Set([
        "compact",
        "normal",
        "large",
        "extra-large"
    ]);

    const validThemes = new Set([
        "normal",
        "cold",
        "warm",
        "dark"
    ]);

    let lastAppliedSignature = "";
    let enhancedLoadRegistered = false;

    function normalize(value) {
        const source =
            value && typeof value === "object"
                ? value
                : {};

        return {
            fontScale:
                validFonts.has(source.fontScale)
                    ? source.fontScale
                    : defaults.fontScale,

            colorTheme:
                validThemes.has(source.colorTheme)
                    ? source.colorTheme
                    : defaults.colorTheme
        };
    }

    function read() {
        try {
            const value =
                window.localStorage.getItem(storageKey);

            return value
                ? normalize(JSON.parse(value))
                : { ...defaults };
        } catch {
            return { ...defaults };
        }
    }

    function notify(settings, source) {
        window.dispatchEvent(
            new CustomEvent(
                "navi:appearancechange",
                {
                    detail: {
                        fontScale: settings.fontScale,
                        colorTheme: settings.colorTheme,
                        source: source || "unknown"
                    }
                }
            )
        );
    }

    function apply(value, source) {
        const settings = normalize(value);
        const root = document.documentElement;

        const signature =
            `${settings.fontScale}|${settings.colorTheme}`;

        const rootIsOutOfSync =
            root.getAttribute("data-navi-font") !==
                settings.fontScale ||
            root.getAttribute("data-navi-theme") !==
                settings.colorTheme;

        const preferencesChanged =
            signature !== lastAppliedSignature;

        root.setAttribute(
            "data-navi-font",
            settings.fontScale
        );

        root.setAttribute(
            "data-navi-theme",
            settings.colorTheme
        );

        root.style.colorScheme =
            settings.colorTheme === "dark"
                ? "dark"
                : "light";

        /*
         * Solo notifica cuando el valor cambió o cuando otra capa
         * visual alteró los atributos del elemento raíz.
         */
        if (preferencesChanged || rootIsOutOfSync) {
            notify(settings, source);
        }

        lastAppliedSignature = signature;

        return settings;
    }

    function save(value) {
        const settings = normalize(value);

        try {
            window.localStorage.setItem(
                storageKey,
                JSON.stringify(settings)
            );
        } catch {
            /*
             * La apariencia se aplica durante la sesión aunque el
             * navegador no permita utilizar localStorage.
             */
        }

        return apply(settings, "local-change");
    }

    function get() {
        return apply(
            read(),
            "manual-read"
        );
    }

    function sync() {
        return apply(
            read(),
            "synchronization"
        );
    }

    function setFontScale(value) {
        const settings = read();

        settings.fontScale =
            validFonts.has(value)
                ? value
                : defaults.fontScale;

        return save(settings);
    }

    function setColorTheme(value) {
        const settings = read();

        settings.colorTheme =
            validThemes.has(value)
                ? value
                : defaults.colorTheme;

        return save(settings);
    }

    function reset() {
        try {
            window.localStorage.removeItem(
                storageKey
            );
        } catch {
            /*
             * Se aplican los valores predeterminados aunque no sea
             * posible eliminar el elemento persistido.
             */
        }

        return apply(
            { ...defaults },
            "reset"
        );
    }

    /*
     * Sincronización entre pestañas del mismo navegador.
     * El evento storage se ejecuta en las otras pestañas abiertas.
     */
    window.addEventListener(
        "storage",
        function (event) {
            if (
                event.storageArea === window.localStorage &&
                (
                    event.key === storageKey ||
                    event.key === null
                )
            ) {
                sync();
            }
        }
    );

    /*
     * Al regresar a una pestaña ya abierta se consulta nuevamente
     * localStorage para tomar cambios hechos en otra pestaña.
     */
    window.addEventListener(
        "focus",
        sync
    );

    window.addEventListener(
        "pageshow",
        sync
    );

    document.addEventListener(
        "visibilitychange",
        function () {
            if (!document.hidden) {
                sync();
            }
        }
    );

    /*
     * Blazor Enhanced Navigation puede cambiar la vista sin crear
     * un documento nuevo. Después de cada navegación se confirma
     * que los atributos globales continúan aplicados.
     */
    function registerEnhancedLoad() {
        if (enhancedLoadRegistered) {
            return true;
        }

        if (
            window.Blazor &&
            typeof window.Blazor.addEventListener === "function"
        ) {
            window.Blazor.addEventListener(
                "enhancedload",
                sync
            );

            enhancedLoadRegistered = true;
            return true;
        }

        return false;
    }

    function tryRegisterEnhancedLoad(attempt) {
        if (registerEnhancedLoad()) {
            return;
        }

        if (attempt >= 20) {
            return;
        }

        window.setTimeout(
            function () {
                tryRegisterEnhancedLoad(attempt + 1);
            },
            250
        );
    }

    document.addEventListener(
        "DOMContentLoaded",
        function () {
            tryRegisterEnhancedLoad(0);
            sync();
        },
        { once: true }
    );

    window.addEventListener(
        "load",
        function () {
            tryRegisterEnhancedLoad(0);
            sync();
        },
        { once: true }
    );

    window.naviUiPreferences = {
        get,
        sync,
        setFontScale,
        setColorTheme,
        reset
    };

    /*
     * Aplicación inicial antes de que Blazor renderice la interfaz.
     */
    apply(
        read(),
        "initial-load"
    );
})();
