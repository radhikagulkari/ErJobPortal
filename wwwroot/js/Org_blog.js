document.addEventListener("DOMContentLoaded", function () {

    /* =====================================================
       SMOOTH SCROLL
    ====================================================== */

    const exploreButton =
        document.querySelector(
            '.why-secondary-btn[href="#benefits"]'
        );

    if (exploreButton) {

        exploreButton.addEventListener("click", function (e) {

            e.preventDefault();

            const target =
                document.getElementById("benefits");

            if (target) {

                target.scrollIntoView({
                    behavior: "smooth",
                    block: "start"
                });

            }

        });

    }


    /* =====================================================
       SCROLL REVEAL
    ====================================================== */

    const revealItems =
        document.querySelectorAll(
            ".benefit-card, .step-item, .success-box, .cta-inner"
        );


    revealItems.forEach(function (item) {

        item.style.opacity = "0";
        item.style.transform = "translateY(25px)";
        item.style.transition =
            "opacity .6s ease, transform .6s ease";

    });


    const revealObserver =
        new IntersectionObserver(
            function (entries, observer) {

                entries.forEach(function (entry) {

                    if (!entry.isIntersecting) {
                        return;
                    }

                    entry.target.style.opacity = "1";
                    entry.target.style.transform =
                        "translateY(0)";

                    observer.unobserve(entry.target);

                });

            },
            {
                threshold: 0.12
            }
        );


    revealItems.forEach(function (item) {

        revealObserver.observe(item);

    });


    /* =====================================================
       STAGGER BENEFIT CARDS
    ====================================================== */

    const benefitCards =
        document.querySelectorAll(".benefit-card");


    benefitCards.forEach(function (card, index) {

        card.style.transitionDelay =
            (index * 80) + "ms";

    });


    /* =====================================================
       STAGGER STEPS
    ====================================================== */

    const steps =
        document.querySelectorAll(".step-item");


    steps.forEach(function (step, index) {

        step.style.transitionDelay =
            (index * 100) + "ms";

    });


    /* =====================================================
       BUTTON ARROW ANIMATION
    ====================================================== */

    const buttons =
        document.querySelectorAll(
            ".why-primary-btn, .cta-button"
        );


    buttons.forEach(function (button) {

        button.addEventListener("mouseenter", function () {

            const icon =
                button.querySelector("i");

            if (icon) {

                icon.style.transform =
                    "translateX(4px)";

                icon.style.transition =
                    "transform .2s ease";

            }

        });


        button.addEventListener("mouseleave", function () {

            const icon =
                button.querySelector("i");

            if (icon) {

                icon.style.transform =
                    "translateX(0)";

            }

        });

    });


});