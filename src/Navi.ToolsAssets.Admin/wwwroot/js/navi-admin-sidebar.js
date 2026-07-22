(function () {
    const stateId = "navi-sidebar-state";
    const sidebarId = "navi-admin-sidebar";
    const rootSelector =
        ".navi-layout-shell.navi-admin-layout-v4";
    const storageKey =
        "navi-admin-sidebar-collapsed";

    function isDesktop() {
        return window.matchMedia(
            "(min-width: 900px)"
        ).matches;
    }

    function getState() {
        return document.getElementById(stateId);
    }

    function getSidebar() {
        return document.getElementById(sidebarId);
    }

    function getRoot() {
        return document.querySelector(rootSelector);
    }

    function saveDesktopState(collapsed) {
        try {
            window.localStorage.setItem(
                storageKey,
                collapsed
                    ? "true"
                    : "false"
            );
        } catch {
        }
    }

    function readDesktopState() {
        try {
            return window.localStorage.getItem(
                storageKey
            ) === "true";
        } catch {
            return false;
        }
    }

    function syncLayout(options) {
        const settings = options || {};
        const state = getState();
        const root = getRoot();

        if (!state || !root) {
            return;
        }

        if (isDesktop()) {
            root.classList.toggle(
                "navi-sidebar-collapsed",
                state.checked
            );

            root.classList.remove(
                "navi-sidebar-mobile-open"
            );

            if (settings.persist !== false) {
                saveDesktopState(state.checked);
            }

            return;
        }

        root.classList.remove(
            "navi-sidebar-collapsed"
        );

        root.classList.toggle(
            "navi-sidebar-mobile-open",
            state.checked
        );
    }

    function expandDesktopSidebar() {
        const state = getState();

        if (
            !state ||
            !isDesktop() ||
            !state.checked
        ) {
            return false;
        }

        state.checked = false;

        state.dispatchEvent(
            new Event(
                "change",
                {
                    bubbles: true
                }
            )
        );

        return true;
    }

    function initializeState() {
        const state = getState();

        if (!state) {
            return;
        }

        if (isDesktop()) {
            state.checked = readDesktopState();
        } else {
            state.checked = false;
        }

        syncLayout({
            persist: false
        });
    }

    document.addEventListener(
        "change",
        function (event) {
            if (
                event.target &&
                event.target.id === stateId
            ) {
                syncLayout();
            }
        },
        true
    );

    document.addEventListener(
        "click",
        function (event) {
            if (!isDesktop()) {
                return;
            }

            const state = getState();
            const sidebar = getSidebar();
            const target = event.target;

            if (
                !state ||
                !sidebar ||
                !state.checked ||
                !target ||
                !target.closest
            ) {
                return;
            }

            const link = target.closest(
                "a.nav-link"
            );

            if (
                !link ||
                !sidebar.contains(link)
            ) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            expandDesktopSidebar();

            window.requestAnimationFrame(
                function () {
                    try {
                        link.focus({
                            preventScroll: true
                        });
                    } catch {
                        link.focus();
                    }
                }
            );
        },
        true
    );

    document.addEventListener(
        "keydown",
        function (event) {
            if (
                !isDesktop() ||
                (
                    event.key !== "Enter" &&
                    event.key !== " "
                )
            ) {
                return;
            }

            const state = getState();
            const sidebar = getSidebar();
            const target = event.target;

            if (
                !state ||
                !sidebar ||
                !state.checked ||
                !target ||
                !target.matches ||
                !target.matches("a.nav-link") ||
                !sidebar.contains(target)
            ) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();

            expandDesktopSidebar();

            window.requestAnimationFrame(
                function () {
                    target.focus();
                }
            );
        },
        true
    );

    window.addEventListener(
        "resize",
        function () {
            const state = getState();

            if (!state) {
                return;
            }

            if (!isDesktop()) {
                state.checked = false;
            } else {
                state.checked =
                    readDesktopState();
            }

            syncLayout({
                persist: false
            });
        }
    );

    if (
        document.readyState === "loading"
    ) {
        document.addEventListener(
            "DOMContentLoaded",
            initializeState,
            {
                once: true
            }
        );
    } else {
        initializeState();
    }

    const observer =
        new MutationObserver(
            function () {
                const root = getRoot();
                const state = getState();

                if (
                    root &&
                    state &&
                    !root.dataset
                        .naviSidebarInitialized
                ) {
                    root.dataset
                        .naviSidebarInitialized =
                        "true";

                    initializeState();
                }
            }
        );

    observer.observe(
        document.documentElement,
        {
            childList: true,
            subtree: true
        }
    );
})();
