(() => {
    "use strict";

    const ROOT_SELECTOR =
        ".navi-mobile-home-image-preview-v80";

    const VIEWPORT_SELECTOR =
        "[data-navi-home-image-viewport]";

    const IMAGE_SELECTOR =
        "[data-navi-home-image]";

    const ACTION_SELECTOR =
        "[data-navi-home-image-action]";

    const BODY_LOCK_CLASS =
        "navi-mobile-home-image-preview-v80-open";

    const MINIMUM_ZOOM =
        1;

    const MAXIMUM_ZOOM =
        4;

    const ZOOM_STEP =
        0.25;

    const stateByRoot =
        new WeakMap();


    function clamp(
        value,
        minimum,
        maximum) {

        return Math.min(
            maximum,
            Math.max(
                minimum,
                value
            )
        );
    }


    function removeLegacyBodyLock() {

        if (
            document.body
        ) {
            document.body.classList.remove(
                BODY_LOCK_CLASS
            );
        }
    }


    function getRoot(
        element) {

        if (
            !(
                element
                instanceof Element
            )
        ) {
            return null;
        }

        return element.closest(
            ROOT_SELECTOR
        );
    }


    function getElements(
        root) {

        return {
            viewport:
                root.querySelector(
                    VIEWPORT_SELECTOR
                ),

            image:
                root.querySelector(
                    IMAGE_SELECTOR
                ),

            zoomValue:
                root.querySelector(
                    '[data-navi-home-image-action="reset"]'
                )
        };
    }


    function getState(
        root) {

        let state =
            stateByRoot.get(
                root
            );

        if (
            state
        ) {
            return state;
        }

        state = {
            zoom: 1,
            x: 0,
            y: 0,

            naturalWidth: 0,
            naturalHeight: 0,

            pointers:
                new Map(),

            dragStartX: 0,
            dragStartY: 0,

            dragOriginX: 0,
            dragOriginY: 0,

            pinchDistance: 0,
            pinchZoom: 1
        };

        stateByRoot.set(
            root,
            state
        );

        return state;
    }


    function updateZoomText(
        zoomButton,
        zoom) {

        if (
            !(
                zoomButton
                instanceof HTMLButtonElement
            )
        ) {
            return;
        }

        const nextText =
            `${Math.round(zoom * 100)}%`;

        const currentText =
            (
                zoomButton.textContent
                ??
                ""
            ).trim();

        if (
            currentText
            ===
            nextText
        ) {
            return;
        }

        zoomButton.textContent =
            nextText;
    }


    function prepareImage(
        root) {

        const {
            viewport,
            image
        } =
            getElements(
                root
            );

        if (
            !(
                viewport
                instanceof HTMLElement
            )
            ||
            !(
                image
                instanceof HTMLImageElement
            )
        ) {
            return;
        }

        image.loading =
            "eager";

        image.decoding =
            "auto";

        image.fetchPriority =
            "high";

        image.draggable =
            false;

        if (
            image.srcset
        ) {
            image.sizes =
                `${Math.max(
                    window.innerWidth,
                    viewport.clientWidth
                    *
                    MAXIMUM_ZOOM
                )}px`;
        }

        const state =
            getState(
                root
            );

        state.naturalWidth =
            image.naturalWidth
            ||
            image.width
            ||
            image.clientWidth
            ||
            viewport.clientWidth;

        state.naturalHeight =
            image.naturalHeight
            ||
            image.height
            ||
            image.clientHeight
            ||
            viewport.clientHeight;

        image.style.width =
            `${state.naturalWidth}px`;

        image.style.height =
            `${state.naturalHeight}px`;

        image.style.maxWidth =
            "none";

        image.style.maxHeight =
            "none";

        image.style.objectFit =
            "initial";

        image.style.imageRendering =
            "auto";

        image.style.transformOrigin =
            "center center";

        image.style.backfaceVisibility =
            "visible";

        image.style.willChange =
            "auto";

        image.style.userSelect =
            "none";

        image.style.webkitUserDrag =
            "none";
    }


    function getBaseScale(
        viewport,
        state) {

        const naturalWidth =
            Math.max(
                1,
                state.naturalWidth
            );

        const naturalHeight =
            Math.max(
                1,
                state.naturalHeight
            );

        const availableWidth =
            Math.max(
                1,
                viewport.clientWidth
            );

        const availableHeight =
            Math.max(
                1,
                viewport.clientHeight
            );

        return Math.min(
            availableWidth
            /
            naturalWidth,

            availableHeight
            /
            naturalHeight,

            1
        );
    }


    function getPanBounds(
        viewport,
        state,
        totalScale) {

        const displayedWidth =
            state.naturalWidth
            *
            totalScale;

        const displayedHeight =
            state.naturalHeight
            *
            totalScale;

        return {
            x:
                Math.max(
                    0,
                    (
                        displayedWidth
                        -
                        viewport.clientWidth
                    )
                    /
                    2
                ),

            y:
                Math.max(
                    0,
                    (
                        displayedHeight
                        -
                        viewport.clientHeight
                    )
                    /
                    2
                )
        };
    }


    function applyTransform(
        root) {

        if (
            !(
                root
                instanceof HTMLElement
            )
        ) {
            return;
        }

        const state =
            getState(
                root
            );

        const {
            viewport,
            image,
            zoomValue
        } =
            getElements(
                root
            );

        updateZoomText(
            zoomValue,
            state.zoom
        );

        if (
            !(
                viewport
                instanceof HTMLElement
            )
            ||
            !(
                image
                instanceof HTMLImageElement
            )
        ) {
            return;
        }

        if (
            state.naturalWidth
            <=
            0
            ||
            state.naturalHeight
            <=
            0
        ) {
            prepareImage(
                root
            );
        }

        const baseScale =
            getBaseScale(
                viewport,
                state
            );

        const totalScale =
            baseScale
            *
            state.zoom;

        const bounds =
            getPanBounds(
                viewport,
                state,
                totalScale
            );

        state.x =
            clamp(
                state.x,
                -bounds.x,
                bounds.x
            );

        state.y =
            clamp(
                state.y,
                -bounds.y,
                bounds.y
            );

        if (
            state.zoom
            <=
            1
        ) {
            state.x =
                0;

            state.y =
                0;
        }

        image.style.left =
            `calc(50% + ${state.x}px)`;

        image.style.top =
            `calc(50% + ${state.y}px)`;

        image.style.transform =
            `translate(-50%, -50%) scale(${totalScale})`;

        viewport.classList.toggle(
            "is-zoomed",
            state.zoom
            >
            1.001
        );
    }


    function setZoom(
        root,
        nextZoom) {

        const state =
            getState(
                root
            );

        state.zoom =
            clamp(
                nextZoom,
                MINIMUM_ZOOM,
                MAXIMUM_ZOOM
            );

        if (
            state.zoom
            <=
            1
        ) {
            state.x =
                0;

            state.y =
                0;
        }

        applyTransform(
            root
        );
    }


    function resetImage(
        root) {

        if (
            !root
        ) {
            return;
        }

        const state =
            getState(
                root
            );

        state.zoom =
            1;

        state.x =
            0;

        state.y =
            0;

        state.dragStartX =
            0;

        state.dragStartY =
            0;

        state.dragOriginX =
            0;

        state.dragOriginY =
            0;

        state.pinchDistance =
            0;

        state.pinchZoom =
            1;

        state.pointers.clear();

        const {
            viewport
        } =
            getElements(
                root
            );

        if (
            viewport
            instanceof HTMLElement
        ) {
            viewport.classList.remove(
                "is-panning"
            );

            viewport.classList.remove(
                "is-zoomed"
            );
        }

        prepareImage(
            root
        );

        applyTransform(
            root
        );
    }


    function distanceBetween(
        first,
        second) {

        return Math.hypot(
            second.x
            -
            first.x,

            second.y
            -
            first.y
        );
    }


    function findPointerRoot(
        pointerId) {

        const roots =
            document.querySelectorAll(
                ROOT_SELECTOR
            );

        for (
            const root
            of roots
        ) {
            const state =
                stateByRoot.get(
                    root
                );

            if (
                state
                &&
                state.pointers.has(
                    pointerId
                )
            ) {
                return root;
            }
        }

        return null;
    }


    document.addEventListener(
        "click",
        event => {

            const target =
                event.target;

            if (
                !(
                    target
                    instanceof Element
                )
            ) {
                return;
            }

            const actionButton =
                target.closest(
                    ACTION_SELECTOR
                );

            if (
                !(
                    actionButton
                    instanceof HTMLButtonElement
                )
            ) {
                return;
            }

            const root =
                getRoot(
                    actionButton
                );

            if (
                !root
            ) {
                return;
            }

            const action =
                actionButton.dataset
                    .naviHomeImageAction;

            const state =
                getState(
                    root
                );

            event.preventDefault();
            event.stopPropagation();

            if (
                action
                ===
                "zoom-in"
            ) {
                setZoom(
                    root,
                    state.zoom
                    +
                    ZOOM_STEP
                );

                return;
            }

            if (
                action
                ===
                "zoom-out"
            ) {
                setZoom(
                    root,
                    state.zoom
                    -
                    ZOOM_STEP
                );

                return;
            }

            if (
                action
                ===
                "reset"
            ) {
                resetImage(
                    root
                );
            }
        },
        true
    );


    document.addEventListener(
        "load",
        event => {

            const target =
                event.target;

            if (
                !(
                    target
                    instanceof HTMLImageElement
                )
                ||
                !target.matches(
                    IMAGE_SELECTOR
                )
            ) {
                return;
            }

            const root =
                getRoot(
                    target
                );

            if (
                root
            ) {
                resetImage(
                    root
                );
            }
        },
        true
    );


    document.addEventListener(
        "wheel",
        event => {

            const target =
                event.target;

            if (
                !(
                    target
                    instanceof Element
                )
            ) {
                return;
            }

            const viewport =
                target.closest(
                    VIEWPORT_SELECTOR
                );

            if (
                !(
                    viewport
                    instanceof HTMLElement
                )
            ) {
                return;
            }

            const root =
                getRoot(
                    viewport
                );

            if (
                !root
            ) {
                return;
            }

            event.preventDefault();

            const state =
                getState(
                    root
                );

            const amount =
                event.deltaY
                <
                0
                    ? ZOOM_STEP
                    : -ZOOM_STEP;

            setZoom(
                root,
                state.zoom
                +
                amount
            );
        },
        {
            passive: false,
            capture: true
        }
    );


    document.addEventListener(
        "pointerdown",
        event => {

            const target =
                event.target;

            if (
                !(
                    target
                    instanceof Element
                )
            ) {
                return;
            }

            const viewport =
                target.closest(
                    VIEWPORT_SELECTOR
                );

            if (
                !(
                    viewport
                    instanceof HTMLElement
                )
            ) {
                return;
            }

            const root =
                getRoot(
                    viewport
                );

            if (
                !root
            ) {
                return;
            }

            if (
                event.pointerType
                ===
                "mouse"
                &&
                event.button
                !==
                0
            ) {
                return;
            }

            prepareImage(
                root
            );

            const state =
                getState(
                    root
                );

            state.pointers.set(
                event.pointerId,
                {
                    x:
                        event.clientX,

                    y:
                        event.clientY
                }
            );

            try {
                viewport.setPointerCapture(
                    event.pointerId
                );
            }
            catch {
            }

            if (
                state.pointers.size
                ===
                1
            ) {
                state.dragStartX =
                    event.clientX;

                state.dragStartY =
                    event.clientY;

                state.dragOriginX =
                    state.x;

                state.dragOriginY =
                    state.y;

                if (
                    state.zoom
                    >
                    1
                ) {
                    viewport.classList.add(
                        "is-panning"
                    );
                }
            }

            if (
                state.pointers.size
                ===
                2
            ) {
                const points =
                    Array.from(
                        state.pointers.values()
                    );

                state.pinchDistance =
                    distanceBetween(
                        points[0],
                        points[1]
                    );

                state.pinchZoom =
                    state.zoom;
            }

            event.preventDefault();
        },
        true
    );


    document.addEventListener(
        "pointermove",
        event => {

            const root =
                findPointerRoot(
                    event.pointerId
                );

            if (
                !root
            ) {
                return;
            }

            const state =
                getState(
                    root
                );

            const {
                viewport
            } =
                getElements(
                    root
                );

            state.pointers.set(
                event.pointerId,
                {
                    x:
                        event.clientX,

                    y:
                        event.clientY
                }
            );

            if (
                state.pointers.size
                >=
                2
            ) {
                const points =
                    Array.from(
                        state.pointers.values()
                    );

                const currentDistance =
                    distanceBetween(
                        points[0],
                        points[1]
                    );

                if (
                    state.pinchDistance
                    >
                    0
                ) {
                    state.zoom =
                        clamp(
                            state.pinchZoom
                            *
                            (
                                currentDistance
                                /
                                state.pinchDistance
                            ),
                            MINIMUM_ZOOM,
                            MAXIMUM_ZOOM
                        );

                    applyTransform(
                        root
                    );
                }
            }
            else if (
                state.pointers.size
                ===
                1
                &&
                state.zoom
                >
                1
            ) {
                state.x =
                    state.dragOriginX
                    +
                    (
                        event.clientX
                        -
                        state.dragStartX
                    );

                state.y =
                    state.dragOriginY
                    +
                    (
                        event.clientY
                        -
                        state.dragStartY
                    );

                if (
                    viewport
                    instanceof HTMLElement
                ) {
                    viewport.classList.add(
                        "is-panning"
                    );
                }

                applyTransform(
                    root
                );
            }

            event.preventDefault();
        },
        {
            passive: false,
            capture: true
        }
    );


    function finishPointer(
        event) {

        const root =
            findPointerRoot(
                event.pointerId
            );

        if (
            !root
        ) {
            return;
        }

        const state =
            getState(
                root
            );

        const {
            viewport
        } =
            getElements(
                root
            );

        state.pointers.delete(
            event.pointerId
        );

        if (
            viewport
            instanceof HTMLElement
        ) {
            viewport.classList.remove(
                "is-panning"
            );

            try {
                if (
                    viewport.hasPointerCapture(
                        event.pointerId
                    )
                ) {
                    viewport.releasePointerCapture(
                        event.pointerId
                    );
                }
            }
            catch {
            }
        }

        if (
            state.pointers.size
            ===
            1
        ) {
            const remainingPointer =
                Array.from(
                    state.pointers.values()
                )[0];

            state.dragStartX =
                remainingPointer.x;

            state.dragStartY =
                remainingPointer.y;

            state.dragOriginX =
                state.x;

            state.dragOriginY =
                state.y;
        }

        if (
            state.pointers.size
            <
            2
        ) {
            state.pinchDistance =
                0;

            state.pinchZoom =
                state.zoom;
        }

        applyTransform(
            root
        );
    }


    document.addEventListener(
        "pointerup",
        finishPointer,
        true
    );


    document.addEventListener(
        "pointercancel",
        finishPointer,
        true
    );


    document.addEventListener(
        "lostpointercapture",
        finishPointer,
        true
    );


    document.addEventListener(
        "dragstart",
        event => {

            const target =
                event.target;

            if (
                target
                instanceof Element
                &&
                target.closest(
                    IMAGE_SELECTOR
                )
            ) {
                event.preventDefault();
            }
        },
        true
    );


    document.addEventListener(
        "dblclick",
        event => {

            const target =
                event.target;

            if (
                !(
                    target
                    instanceof Element
                )
            ) {
                return;
            }

            const viewport =
                target.closest(
                    VIEWPORT_SELECTOR
                );

            if (
                !viewport
            ) {
                return;
            }

            const root =
                getRoot(
                    viewport
                );

            if (
                root
            ) {
                event.preventDefault();

                resetImage(
                    root
                );
            }
        },
        true
    );


    document.addEventListener(
        "keydown",
        event => {

            const root =
                document.querySelector(
                    ROOT_SELECTOR
                );

            if (
                !root
            ) {
                return;
            }

            if (
                event.key
                ===
                "Escape"
            ) {
                const closeButton =
                    root.querySelector(
                        ".navi-mobile-home-image-preview-v80__close"
                    );

                if (
                    closeButton
                    instanceof HTMLButtonElement
                ) {
                    closeButton.click();
                }

                return;
            }

            const state =
                getState(
                    root
                );

            if (
                event.key
                ===
                "+"
                ||
                event.key
                ===
                "="
            ) {
                event.preventDefault();

                setZoom(
                    root,
                    state.zoom
                    +
                    ZOOM_STEP
                );

                return;
            }

            if (
                event.key
                ===
                "-"
            ) {
                event.preventDefault();

                setZoom(
                    root,
                    state.zoom
                    -
                    ZOOM_STEP
                );

                return;
            }

            if (
                event.key
                ===
                "0"
            ) {
                event.preventDefault();

                resetImage(
                    root
                );
            }
        },
        true
    );


    window.addEventListener(
        "resize",
        () => {

            const roots =
                document.querySelectorAll(
                    ROOT_SELECTOR
                );

            for (
                const root
                of roots
            ) {
                prepareImage(
                    root
                );

                applyTransform(
                    root
                );
            }
        }
    );


    window.addEventListener(
        "pagehide",
        removeLegacyBodyLock
    );


    window.addEventListener(
        "pageshow",
        removeLegacyBodyLock
    );


    if (
        document.readyState
        ===
        "loading"
    ) {
        document.addEventListener(
            "DOMContentLoaded",
            () => {

                removeLegacyBodyLock();

                const roots =
                    document.querySelectorAll(
                        ROOT_SELECTOR
                    );

                for (
                    const root
                    of roots
                ) {
                    const {
                        image
                    } =
                        getElements(
                            root
                        );

                    if (
                        image
                        instanceof HTMLImageElement
                        &&
                        image.complete
                        &&
                        image.naturalWidth
                        >
                        0
                    ) {
                        resetImage(
                            root
                        );
                    }
                }
            },
            {
                once: true
            }
        );
    }
    else {
        removeLegacyBodyLock();
    }


    window.NaviMobileHomeImagePreviewV80 = {
        reset() {

            const root =
                document.querySelector(
                    ROOT_SELECTOR
                );

            if (
                root
            ) {
                resetImage(
                    root
                );
            }
        }
    };
})();
