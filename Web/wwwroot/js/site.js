function initBannerScroll() {
    console.log("Initializing banner scroll logic");

    let hasScrolledFromBanner = false;

    window.addEventListener('wheel', function (e) {
        const banner = document.getElementById('banner-section');
        if (!banner) return;

        // Only trigger if:
        // 1. We're at the very top of the page (scrollY < 100)
        // 2. We haven't already triggered this scroll
        // 3. User is scrolling down
        if (window.scrollY < 100 && !hasScrolledFromBanner && e.deltaY > 0) {
            console.log("Scroll down detected on banner, triggering scrollIntoView");
            e.preventDefault();
            hasScrolledFromBanner = true;

            const moviesSection = document.getElementById('movies-section');
            if (moviesSection) {
                moviesSection.scrollIntoView({ behavior: 'smooth' });
            }

            // Reset flag after scroll completes
            setTimeout(() => {
                hasScrolledFromBanner = false;
            }, 1000);
        }
    }, { passive: false });
}
