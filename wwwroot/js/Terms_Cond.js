document.addEventListener("DOMContentLoaded", function () {

    const progressBar =
        document.getElementById("termsProgress");

    const backToTop =
        document.getElementById("termsTop");

    const tocLinks =
        document.querySelectorAll(".toc-link");

    const sections =
        document.querySelectorAll(".terms-section");


    /* =====================================================
       SMOOTH SCROLL
    ====================================================== */

    tocLinks.forEach(function (link) {

        link.addEventListener("click", function (event) {

            event.preventDefault();

            const targetId =
                link.getAttribute("href");

            const target =
                document.querySelector(targetId);

            if (!target) {
                return;
            }

            const headerOffset = 90;

            const targetPosition =
                target.getBoundingClientRect().top +
                window.pageYOffset -
                headerOffset;

            window.scrollTo({
                top: targetPosition,
                behavior: "smooth"
            });

        });

    });


    /* =====================================================
       UPDATE ACTIVE TOC
    ====================================================== */

    function updateActiveSection() {

        let currentSection = "";

        const scrollPosition =
            window.scrollY + 140;


        sections.forEach(function (section) {

            const sectionTop =
                section.offsetTop;

            if (scrollPosition >= sectionTop) {

                currentSection =
                    section.getAttribute("id");

            }

        });


        tocLinks.forEach(function (link) {

            link.classList.remove("active");

            const linkTarget =
                link.getAttribute("href");

            if (linkTarget === "#" + currentSection) {

                link.classList.add("active");

            }

        });

    }


    /* =====================================================
       SCROLL PROGRESS
    ====================================================== */

    function updateProgress() {

        const scrollTop =
            window.scrollY;

        const documentHeight =
            document.documentElement.scrollHeight -
            document.documentElement.clientHeight;

        if (documentHeight <= 0) {

            progressBar.style.width = "0%";

            return;
        }


        const progress =
            (scrollTop / documentHeight) * 100;


        progressBar.style.width =
            Math.min(progress, 100) + "%";

    }


    /* =====================================================
       BACK TO TOP
    ====================================================== */

    function updateBackToTop() {

        if (window.scrollY > 450) {

            backToTop.classList.add("show");

        } else {

            backToTop.classList.remove("show");

        }

    }


    backToTop.addEventListener(
        "click",
        function () {

            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });

        }
    );


    /* =====================================================
       SCROLL EVENT
    ====================================================== */

    let ticking = false;

    window.addEventListener("scroll", function () {

        if (!ticking) {

            window.requestAnimationFrame(function () {

                updateActiveSection();
                updateProgress();
                updateBackToTop();

                ticking = false;

            });

            ticking = true;

        }

    });


    /* =====================================================
       INITIAL
    ====================================================== */

    updateActiveSection();
    updateProgress();
    updateBackToTop();

});