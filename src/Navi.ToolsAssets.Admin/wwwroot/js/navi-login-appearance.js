(function () {
    "use strict";

    if (window.naviLoginAppearance) {
        return;
    }

    const objectUrls =
        new Map();

    function revoke(inputId) {
        const currentUrl =
            objectUrls.get(inputId);

        if (currentUrl) {
            URL.revokeObjectURL(
                currentUrl
            );

            objectUrls.delete(
                inputId
            );
        }
    }

    window.naviLoginAppearance = {
        createPreview: function (inputId) {
            revoke(inputId);

            const input =
                document.getElementById(
                    inputId
                );

            if (
                !input
                ||
                !input.files
                ||
                input.files.length === 0
            ) {
                return null;
            }

            const url =
                URL.createObjectURL(
                    input.files[0]
                );

            objectUrls.set(
                inputId,
                url
            );

            return url;
        },

        clearPreview: function (inputId) {
            revoke(inputId);

            const input =
                document.getElementById(
                    inputId
                );

            if (input) {
                input.value = "";
            }
        }
    };
})();
