window.naviMobileAvailabilityV25 = {
    scrollToManagement: function () {
        window.requestAnimationFrame(function () {
            const section =
                document.getElementById(
                    "mobile-availability-management"
                );

            if (!section) {
                return;
            }

            section.scrollIntoView({
                behavior: "smooth",
                block: "start",
                inline: "nearest"
            });

            window.setTimeout(function () {
                const firstField =
                    section.querySelector(
                        "input:not([disabled]), " +
                        "select:not([disabled]), " +
                        "textarea:not([disabled])"
                    );

                if (firstField) {
                    try {
                        firstField.focus({
                            preventScroll: true
                        });
                    } catch {
                        firstField.focus();
                    }
                }
            }, 420);
        });
    }
};
