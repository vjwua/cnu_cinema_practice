window.HomeHelpers = {
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth' });
        }
    },

    initHeroScroll: function () {
        const hero = document.querySelector('.hero-section');
        if (!hero) return;

        let isScrolling = false;

        const handleScroll = (e) => {
            // Only trigger if we are at the top and scrolling down
            if (window.scrollY < 50 && e.deltaY > 0 && !isScrolling) {
                e.preventDefault();
                isScrolling = true;

                const grid = document.getElementById('movies-grid');
                if (grid) {
                    grid.scrollIntoView({ behavior: 'smooth' });
                    // Reset lock after animation
                    setTimeout(() => {
                        isScrolling = false;
                    }, 1000);
                }
            }
        };

        // Add passive: false to allow preventDefault
        window.addEventListener('wheel', handleScroll, { passive: false });
    }
};
