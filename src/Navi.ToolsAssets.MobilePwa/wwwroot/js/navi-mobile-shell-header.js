(function () {
    const registrations = new Map();

    function invokeSafely(
        dotNetReference,
        methodName
    ) {
        try {
            const invocation =
                dotNetReference.invokeMethodAsync(
                    methodName
                );

            if (
                invocation &&
                typeof invocation.catch === "function"
            ) {
                invocation.catch(function () { });
            }
        } catch {
        }
    }

    function unregister(rootId) {
        const registration =
            registrations.get(rootId);

        if (!registration) {
            return;
        }

        document.removeEventListener(
            "pointerdown",
            registration.onPointerDown,
            true
        );

        document.removeEventListener(
            "keydown",
            registration.onKeyDown,
            true
        );

        window.removeEventListener(
            "blur",
            registration.onWindowBlur
        );

        registrations.delete(rootId);
    }

    function register(
        dotNetReference,
        rootId
    ) {
        unregister(rootId);

        const root =
            document.getElementById(rootId);

        if (!root) {
            return false;
        }

        const onPointerDown = function (event) {
            const target = event.target;

            if (
                !target ||
                typeof target.closest !== "function"
            ) {
                invokeSafely(
                    dotNetReference,
                    "CloseShellMenus"
                );

                return;
            }

            const control = target.closest(
                "[data-navi-mobile-shell-control]"
            );

            const popup = target.closest(
                "[data-navi-mobile-shell-popup]"
            );

            if (!control && !popup) {
                invokeSafely(
                    dotNetReference,
                    "CloseShellMenus"
                );
            }
        };

        const onKeyDown = function (event) {
            if (event.key === "Escape") {
                invokeSafely(
                    dotNetReference,
                    "CloseShellMenus"
                );
            }
        };

        const onWindowBlur = function () {
            invokeSafely(
                dotNetReference,
                "CloseShellMenus"
            );
        };

        document.addEventListener(
            "pointerdown",
            onPointerDown,
            true
        );

        document.addEventListener(
            "keydown",
            onKeyDown,
            true
        );

        window.addEventListener(
            "blur",
            onWindowBlur
        );

        registrations.set(
            rootId,
            {
                onPointerDown,
                onKeyDown,
                onWindowBlur
            }
        );

        return true;
    }

    window.naviMobileShellHeader = {
        register,
        unregister
    };
})();
