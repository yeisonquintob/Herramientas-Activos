(function () {
    "use strict";

    var registrations = {};

    function register(id, root, dotNetReference) {
        unregister(id);

        function closeWhenOutside(event) {
            if (!root || root.contains(event.target)) {
                return;
            }

            dotNetReference.invokeMethodAsync(
                "CloseFromOutside"
            );
        }

        document.addEventListener(
            "pointerdown",
            closeWhenOutside,
            true
        );

        document.addEventListener(
            "focusin",
            closeWhenOutside,
            true
        );

        registrations[id] = {
            closeWhenOutside: closeWhenOutside
        };
    }

    function unregister(id) {
        var registration = registrations[id];

        if (!registration) {
            return;
        }

        document.removeEventListener(
            "pointerdown",
            registration.closeWhenOutside,
            true
        );

        document.removeEventListener(
            "focusin",
            registration.closeWhenOutside,
            true
        );

        delete registrations[id];
    }

    window.naviMobileKpiOverflow = {
        register: register,
        unregister: unregister
    };
})();
