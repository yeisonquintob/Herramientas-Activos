(function () {
    const registrations = new Map();

    function invokeSafely(dotNetReference, methodName) {
        try {
            const invocation =
                dotNetReference.invokeMethodAsync(methodName);

            if (invocation && invocation.catch) {
                invocation.catch(function () { });
            }
        } catch {
        }
    }

    function unregister(rootId) {
        const registration = registrations.get(rootId);

        if (!registration) {
            return;
        }

        document.removeEventListener(
            "pointerdown",
            registration.onPointerDown,
            true);

        document.removeEventListener(
            "keydown",
            registration.onKeyDown,
            true);

        window.removeEventListener(
            "focus",
            registration.onWindowFocus);

        registrations.delete(rootId);
    }

    function register(dotNetReference, rootId) {
        unregister(rootId);

        const root = document.getElementById(rootId);

        if (!root) {
            return false;
        }

        const onPointerDown = function (event) {
            const target = event.target;

            if (!target || !target.closest) {
                invokeSafely(
                    dotNetReference,
                    "CloseHeaderMenus");

                return;
            }

            const menu =
                target.closest("[data-navi-header-menu]");

            if (!menu || !root.contains(menu)) {
                invokeSafely(
                    dotNetReference,
                    "CloseHeaderMenus");
            }
        };

        const onKeyDown = function (event) {
            if (event.key === "Escape") {
                invokeSafely(
                    dotNetReference,
                    "CloseHeaderMenus");
            }
        };

        const onWindowFocus = function () {
            invokeSafely(
                dotNetReference,
                "RefreshHeaderNotifications");
        };

        document.addEventListener(
            "pointerdown",
            onPointerDown,
            true);

        document.addEventListener(
            "keydown",
            onKeyDown,
            true);

        window.addEventListener(
            "focus",
            onWindowFocus);

        registrations.set(rootId, {
            onPointerDown,
            onKeyDown,
            onWindowFocus
        });

        return true;
    }

    window.naviAdminHeaderMenus = {
        register,
        unregister
    };
})();
