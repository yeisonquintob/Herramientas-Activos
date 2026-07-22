(function () {
    "use strict";

    var initializationKey =
        "__naviMobileUiPreferencesInitialized";

    if (window[initializationKey]) {
        if (
            window.naviMobileUiPreferences &&
            typeof window.naviMobileUiPreferences.sync === "function"
        ) {
            window.naviMobileUiPreferences.sync();
        }

        return;
    }

    window[initializationKey] = true;

    var storageKey = "navi-mobile-ui-preferences-v1";

    var defaultPreferences = {
        fontScale: "normal",
        colorTheme: "normal"
    };

    var validFontScales = [
        "compact",
        "normal",
        "large",
        "extra-large"
    ];

    var validColorThemes = [
        "normal",
        "cold",
        "warm",
        "dark"
    ];

    var lastAppliedSignature = "";
    var enhancedLoadRegistered = false;

    function contains(values, value) {
        return values.indexOf(value) >= 0;
    }

    function normalize(value) {
        var source =
            value && typeof value === "object"
                ? value
                : {};

        return {
            fontScale:
                contains(
                    validFontScales,
                    source.fontScale
                )
                    ? source.fontScale
                    : defaultPreferences.fontScale,

            colorTheme:
                contains(
                    validColorThemes,
                    source.colorTheme
                )
                    ? source.colorTheme
                    : defaultPreferences.colorTheme
        };
    }

    function readPreferences() {
        try {
            var stored =
                window.localStorage.getItem(
                    storageKey
                );

            if (!stored) {
                return normalize(
                    defaultPreferences
                );
            }

            return normalize(
                JSON.parse(stored)
            );
        } catch (error) {
            return normalize(
                defaultPreferences
            );
        }
    }

    function applyPreferences(value, source) {
        var preferences =
            normalize(value);

        var root =
            document.documentElement;

        var signature =
            preferences.fontScale + "|" +
            preferences.colorTheme;

        var rootIsOutOfSync =
            root.getAttribute("data-navi-mobile-font") !==
                preferences.fontScale ||
            root.getAttribute("data-navi-mobile-theme") !==
                preferences.colorTheme;

        var preferencesChanged =
            signature !== lastAppliedSignature;

        root.setAttribute(
            "data-navi-mobile-font",
            preferences.fontScale
        );

        root.setAttribute(
            "data-navi-mobile-theme",
            preferences.colorTheme
        );

        root.style.colorScheme =
            preferences.colorTheme === "dark"
                ? "dark"
                : "light";

        if (preferencesChanged || rootIsOutOfSync) {
            var detail = {
                fontScale: preferences.fontScale,
                colorTheme: preferences.colorTheme,
                source: source || "unknown"
            };

            try {
                window.dispatchEvent(
                    new CustomEvent(
                        "navi:appearancechange",
                        { detail: detail }
                    )
                );
            } catch (error) {
                var event = document.createEvent(
                    "CustomEvent"
                );

                event.initCustomEvent(
                    "navi:appearancechange",
                    false,
                    false,
                    detail
                );

                window.dispatchEvent(event);
            }
        }

        lastAppliedSignature = signature;

        return preferences;
    }

    function savePreferences(value) {
        var preferences =
            normalize(value);

        try {
            window.localStorage.setItem(
                storageKey,
                JSON.stringify(preferences)
            );
        } catch (error) {
            console.warn(
                "No fue posible guardar las preferencias móviles.",
                error
            );
        }

        return applyPreferences(
            preferences,
            "local-change"
        );
    }

    function getPreferences() {
        return applyPreferences(
            readPreferences(),
            "manual-read"
        );
    }

    function syncPreferences() {
        return applyPreferences(
            readPreferences(),
            "synchronization"
        );
    }

    function setFontScale(value) {
        var preferences =
            readPreferences();

        preferences.fontScale =
            contains(validFontScales, value)
                ? value
                : defaultPreferences.fontScale;

        return savePreferences(
            preferences
        );
    }

    function setColorTheme(value) {
        var preferences =
            readPreferences();

        preferences.colorTheme =
            contains(validColorThemes, value)
                ? value
                : defaultPreferences.colorTheme;

        return savePreferences(
            preferences
        );
    }

    function resetPreferences() {
        try {
            window.localStorage.removeItem(
                storageKey
            );
        } catch (error) {
            console.warn(
                "No fue posible restablecer las preferencias móviles.",
                error
            );
        }

        return applyPreferences(
            {
                fontScale:
                    defaultPreferences.fontScale,

                colorTheme:
                    defaultPreferences.colorTheme
            },
            "reset"
        );
    }

    window.addEventListener(
        "storage",
        function (event) {
            if (
                event.storageArea === window.localStorage &&
                (event.key === storageKey || event.key === null)
            ) {
                syncPreferences();
            }
        }
    );

    window.addEventListener(
        "focus",
        syncPreferences
    );

    window.addEventListener(
        "pageshow",
        syncPreferences
    );

    document.addEventListener(
        "visibilitychange",
        function () {
            if (!document.hidden) {
                syncPreferences();
            }
        }
    );

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
                syncPreferences
            );

            enhancedLoadRegistered = true;
            return true;
        }

        return false;
    }

    function tryRegisterEnhancedLoad(attempt) {
        if (registerEnhancedLoad() || attempt >= 20) {
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
            syncPreferences();
        },
        { once: true }
    );

    window.addEventListener(
        "load",
        function () {
            tryRegisterEnhancedLoad(0);
            syncPreferences();
        },
        { once: true }
    );

    window.naviMobileUiPreferences = {
        get: getPreferences,
        sync: syncPreferences,
        setFontScale: setFontScale,
        setColorTheme: setColorTheme,
        reset: resetPreferences
    };

    applyPreferences(
        readPreferences(),
        "initial-load"
    );
})();
