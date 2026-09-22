document.addEventListener("DOMContentLoaded", function () {

    const track = document.getElementById("teamTrack");
    const slider = document.getElementById("teamSlider");

    const prevBtn = document.getElementById("teamPrev");
    const nextBtn = document.getElementById("teamNext");

    const dotsContainer = document.getElementById("teamDots");

    const slides = Array.from(
        document.querySelectorAll(".team-slide")
    );


    /* =====================================================
       CHECK ELEMENTS
       ===================================================== */

    if (
        !track ||
        !slider ||
        !prevBtn ||
        !nextBtn ||
        !dotsContainer ||
        !slides.length
    ) {
        return;
    }


    /* =====================================================
       SETTINGS
       ===================================================== */

    const AUTO_PLAY_DELAY = 3500;

    let currentIndex = 0;

    let autoPlayTimer = null;

    let isHovered = false;

    let isAnimating = false;


    /* =====================================================
       GET SLIDES PER VIEW
       ===================================================== */

    function getSlidesPerView() {

        if (window.innerWidth <= 767) {
            return 1;
        }

        if (window.innerWidth <= 991) {
            return 2;
        }

        return 3;
    }


    /* =====================================================
       GET MAX INDEX
       ===================================================== */

    function getMaxIndex() {

        const slidesPerView = getSlidesPerView();

        return Math.max(
            0,
            slides.length - slidesPerView
        );
    }


    /* =====================================================
       GET SLIDE GAP
       ===================================================== */

    function getGap() {

        const trackStyle =
            window.getComputedStyle(track);

        return parseFloat(trackStyle.gap) || 0;
    }


    /* =====================================================
       CREATE DOTS
       ===================================================== */

    function createDots() {

        dotsContainer.innerHTML = "";

        const maxIndex = getMaxIndex();

        for (let i = 0; i <= maxIndex; i++) {

            const dot =
                document.createElement("button");

            dot.type = "button";

            dot.className = "team-dot";

            dot.setAttribute(
                "aria-label",
                "Show team slide " + (i + 1)
            );


            dot.addEventListener(
                "click",
                function () {

                    currentIndex = i;

                    updateSlider();

                    restartAutoPlay();

                }
            );


            dotsContainer.appendChild(dot);

        }

    }


    /* =====================================================
       UPDATE DOTS
       ===================================================== */

    function updateDots() {

        const dots =
            dotsContainer.querySelectorAll(
                ".team-dot"
            );


        dots.forEach(
            function (dot, index) {

                dot.classList.toggle(
                    "active",
                    index === currentIndex
                );

            }
        );

    }


    /* =====================================================
       UPDATE SLIDER
       ===================================================== */

    function updateSlider() {

        const maxIndex =
            getMaxIndex();


        if (currentIndex > maxIndex) {
            currentIndex = 0;
        }


        if (currentIndex < 0) {
            currentIndex = maxIndex;
        }


        const firstSlide =
            slides[0];


        if (!firstSlide) {
            return;
        }


        const slideWidth =
            firstSlide.getBoundingClientRect().width;


        const gap =
            getGap();


        const moveAmount =
            currentIndex *
            (slideWidth + gap);


        track.style.transform =
            "translate3d(-" +
            moveAmount +
            "px, 0, 0)";


        updateDots();

    }


    /* =====================================================
       NEXT SLIDE
       ===================================================== */

    function nextSlide() {

        if (isAnimating) {
            return;
        }


        const maxIndex =
            getMaxIndex();


        if (currentIndex >= maxIndex) {

            currentIndex = 0;

        }
        else {

            currentIndex++;

        }


        updateSlider();

    }


    /* =====================================================
       PREVIOUS SLIDE
       ===================================================== */

    function previousSlide() {

        if (isAnimating) {
            return;
        }


        const maxIndex =
            getMaxIndex();


        if (currentIndex <= 0) {

            currentIndex = maxIndex;

        }
        else {

            currentIndex--;

        }


        updateSlider();

    }


    /* =====================================================
       START AUTO PLAY
       ===================================================== */

    function startAutoPlay() {

        stopAutoPlay();


        if (slides.length <= getSlidesPerView()) {
            return;
        }


        autoPlayTimer =
            setInterval(
                function () {

                    if (!isHovered) {

                        nextSlide();

                    }

                },
                AUTO_PLAY_DELAY
            );

    }


    /* =====================================================
       STOP AUTO PLAY
       ===================================================== */

    function stopAutoPlay() {

        if (autoPlayTimer) {

            clearInterval(
                autoPlayTimer
            );

            autoPlayTimer = null;

        }

    }


    /* =====================================================
       RESTART AUTO PLAY
       ===================================================== */

    function restartAutoPlay() {

        stopAutoPlay();

        startAutoPlay();

    }


    /* =====================================================
       NEXT BUTTON
       ===================================================== */

    nextBtn.addEventListener(
        "click",
        function () {

            nextSlide();

            restartAutoPlay();

        }
    );


    /* =====================================================
       PREVIOUS BUTTON
       ===================================================== */

    prevBtn.addEventListener(
        "click",
        function () {

            previousSlide();

            restartAutoPlay();

        }
    );


    /* =====================================================
       PAUSE ON HOVER
       ===================================================== */

    slider.addEventListener(
        "mouseenter",
        function () {

            isHovered = true;

            stopAutoPlay();

        }
    );


    /* =====================================================
       RESUME AFTER HOVER
       ===================================================== */

    slider.addEventListener(
        "mouseleave",
        function () {

            isHovered = false;

            startAutoPlay();

        }
    );


    /* =====================================================
       TOUCH SUPPORT
       ===================================================== */

    let touchStartX = 0;

    let touchEndX = 0;


    slider.addEventListener(
        "touchstart",
        function (event) {

            touchStartX =
                event.changedTouches[0].screenX;

            stopAutoPlay();

        },
        {
            passive: true
        }
    );


    slider.addEventListener(
        "touchend",
        function (event) {

            touchEndX =
                event.changedTouches[0].screenX;


            const difference =
                touchStartX - touchEndX;


            if (Math.abs(difference) > 50) {

                if (difference > 0) {

                    nextSlide();

                }
                else {

                    previousSlide();

                }

            }


            startAutoPlay();

        },
        {
            passive: true
        }
    );


    /* =====================================================
       RESIZE
       ===================================================== */

    let resizeTimer;


    window.addEventListener(
        "resize",
        function () {

            clearTimeout(
                resizeTimer
            );


            resizeTimer =
                setTimeout(
                    function () {

                        const maxIndex =
                            getMaxIndex();


                        if (
                            currentIndex >
                            maxIndex
                        ) {

                            currentIndex =
                                maxIndex;

                        }


                        createDots();

                        updateSlider();

                        restartAutoPlay();

                    },
                    200
                );

        }
    );


    /* =====================================================
       INITIALIZE
       ===================================================== */

    createDots();

    updateSlider();

    startAutoPlay();

});