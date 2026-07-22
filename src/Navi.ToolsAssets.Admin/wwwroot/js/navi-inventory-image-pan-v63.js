(() => {
    "use strict";

    const areaSelector =
        ".navi-inventory-v58-preview-content";

    const imageSelector =
        "img.navi-inventory-v60-preview-image";

    const zoomControlsSelector =
        ".navi-inventory-v60-zoom-controls";

    let activeArea = null;

    let activePointerId = null;

    let startPointerX = 0;
    let startPointerY = 0;

    let startScrollLeft = 0;
    let startScrollTop = 0;


    function isElement(value) {
        return value instanceof Element;
    }


    function refreshArea(area) {
        if (!(area instanceof HTMLElement)) {
            return;
        }

        window.requestAnimationFrame(() => {
            const hasHorizontalOverflow =
                area.scrollWidth
                >
                area.clientWidth
                +
                2;

            const hasVerticalOverflow =
                area.scrollHeight
                >
                area.clientHeight
                +
                2;

            area.classList.toggle(
                "is-pannable",
                hasHorizontalOverflow
                ||
                hasVerticalOverflow
            );
        });
    }


    function refreshAllAreas() {
        document
            .querySelectorAll(areaSelector)
            .forEach(refreshArea);
    }


    function scheduleRefresh(area) {
        refreshArea(area);

        window.setTimeout(
            () => refreshArea(area),
            40
        );

        window.setTimeout(
            () => refreshArea(area),
            160
        );

        window.setTimeout(
            () => refreshArea(area),
            320
        );
    }


    function stopPanning(event) {
        if (!activeArea) {
            return;
        }

        if (
            event
            &&
            activePointerId !== null
            &&
            event.pointerId !== activePointerId
        ) {
            return;
        }

        activeArea.classList.remove(
            "is-panning"
        );

        try {
            if (
                activePointerId !== null
                &&
                activeArea.hasPointerCapture(
                    activePointerId
                )
            ) {
                activeArea.releasePointerCapture(
                    activePointerId
                );
            }
        }
        catch {
            // No realizar ninguna acción.
        }

        refreshArea(activeArea);

        activeArea = null;
        activePointerId = null;
    }


    document.addEventListener(
        "pointerdown",
        event => {
            if (!isElement(event.target)) {
                return;
            }

            const image =
                event.target.closest(
                    imageSelector
                );

            if (!image) {
                return;
            }

            const area =
                image.closest(
                    areaSelector
                );

            if (!(area instanceof HTMLElement)) {
                return;
            }

            if (
                event.pointerType === "mouse"
                &&
                event.button !== 0
            ) {
                return;
            }

            refreshArea(area);

            const hasOverflow =
                area.scrollWidth
                    >
                    area.clientWidth
                    +
                    2
                ||
                area.scrollHeight
                    >
                    area.clientHeight
                    +
                    2;

            if (!hasOverflow) {
                return;
            }

            event.preventDefault();

            activeArea = area;
            activePointerId = event.pointerId;

            startPointerX = event.clientX;
            startPointerY = event.clientY;

            startScrollLeft = area.scrollLeft;
            startScrollTop = area.scrollTop;

            area.classList.add(
                "is-panning"
            );

            try {
                area.setPointerCapture(
                    event.pointerId
                );
            }
            catch {
                // El arrastre sigue funcionando por delegación.
            }
        },
        true
    );


    document.addEventListener(
        "pointermove",
        event => {
            if (
                !activeArea
                ||
                activePointerId === null
                ||
                event.pointerId !== activePointerId
            ) {
                return;
            }

            event.preventDefault();

            const movementX =
                event.clientX
                -
                startPointerX;

            const movementY =
                event.clientY
                -
                startPointerY;

            activeArea.scrollLeft =
                startScrollLeft
                -
                movementX;

            activeArea.scrollTop =
                startScrollTop
                -
                movementY;
        },
        true
    );


    document.addEventListener(
        "pointerup",
        stopPanning,
        true
    );


    document.addEventListener(
        "pointercancel",
        stopPanning,
        true
    );


    document.addEventListener(
        "lostpointercapture",
        stopPanning,
        true
    );


    document.addEventListener(
        "dragstart",
        event => {
            if (!isElement(event.target)) {
                return;
            }

            if (
                event.target.closest(
                    imageSelector
                )
            ) {
                event.preventDefault();
            }
        },
        true
    );


    document.addEventListener(
        "wheel",
        event => {
            if (!isElement(event.target)) {
                return;
            }

            const area =
                event.target.closest(
                    areaSelector
                );

            if (area) {
                scheduleRefresh(area);
            }
        },
        true
    );


    document.addEventListener(
        "click",
        event => {
            if (!isElement(event.target)) {
                return;
            }

            const controls =
                event.target.closest(
                    zoomControlsSelector
                );

            if (!controls) {
                return;
            }

            const dialog =
                controls.closest(
                    ".navi-inventory-v58-preview-dialog"
                );

            const area =
                dialog?.querySelector(
                    areaSelector
                );

            if (area) {
                scheduleRefresh(area);
            }
        },
        true
    );


    const observer =
        new MutationObserver(
            mutations => {
                let mustRefresh = false;

                for (const mutation of mutations) {
                    if (
                        mutation.type === "childList"
                        ||
                        mutation.type === "attributes"
                    ) {
                        mustRefresh = true;
                        break;
                    }
                }

                if (mustRefresh) {
                    refreshAllAreas();
                }
            }
        );


    function initialize() {
        observer.observe(
            document.body,
            {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: [
                    "style",
                    "class"
                ]
            }
        );

        window.addEventListener(
            "resize",
            refreshAllAreas
        );

        refreshAllAreas();
    }


    if (
        document.readyState === "loading"
    ) {
        document.addEventListener(
            "DOMContentLoaded",
            initialize,
            {
                once: true
            }
        );
    }
    else {
        initialize();
    }
})();
