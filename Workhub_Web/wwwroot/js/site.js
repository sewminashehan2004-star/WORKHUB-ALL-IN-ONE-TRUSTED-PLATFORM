document.addEventListener(
    "DOMContentLoaded",
    function () {

        // =====================================================
        // MOBILE NAVIGATION
        // =====================================================

        const mobileMenuButton =
            document.getElementById(
                "mobileMenuButton"
            );

        const mobileNav =
            document.getElementById(
                "mobileNav"
            );


        if (
            mobileMenuButton &&
            mobileNav
        ) {

            mobileMenuButton.addEventListener(
                "click",
                function () {

                    mobileNav.classList.toggle(
                        "open"
                    );

                }
            );


            mobileNav
                .querySelectorAll("a")
                .forEach(
                    function (link) {

                        link.addEventListener(
                            "click",
                            function () {

                                mobileNav.classList.remove(
                                    "open"
                                );

                            }
                        );

                    }
                );

        }


        // =====================================================
        // HERO SLIDESHOW
        // =====================================================

        const slides =
            document.querySelectorAll(
                ".hero-slide"
            );

        const dots =
            document.querySelectorAll(
                ".hero-dot"
            );

        let currentSlide = 0;
        let slideTimer = null;


        function showSlide(index) {

            if (slides.length === 0) {
                return;
            }


            if (index >= slides.length) {
                index = 0;
            }


            if (index < 0) {
                index = slides.length - 1;
            }


            slides.forEach(
                function (slide) {

                    slide.classList.remove(
                        "active"
                    );

                }
            );


            dots.forEach(
                function (dot) {

                    dot.classList.remove(
                        "active"
                    );

                }
            );


            slides[index].classList.add(
                "active"
            );


            if (dots[index]) {

                dots[index].classList.add(
                    "active"
                );

            }


            currentSlide = index;

        }


        function nextSlide() {

            showSlide(
                currentSlide + 1
            );

        }


        function startSlideshow() {

            if (slideTimer) {

                clearInterval(
                    slideTimer
                );

            }


            slideTimer =
                setInterval(
                    nextSlide,
                    5000
                );

        }


        dots.forEach(
            function (dot, index) {

                dot.addEventListener(
                    "click",
                    function () {

                        showSlide(index);

                        startSlideshow();

                    }
                );

            }
        );


        if (slides.length > 0) {

            showSlide(0);

            startSlideshow();

        }


        // =====================================================
        // HOME SEARCH
        // =====================================================

        const home =
            document.getElementById(
                "workHubHome"
            );

        const searchForm =
            document.getElementById(
                "locationSearchForm"
            );


        if (
            !home ||
            !searchForm
        ) {
            return;
        }


        let apiBase =
            home.dataset.apiBase
            ||
            "https://localhost:7015/";


        if (!apiBase.endsWith("/")) {

            apiBase += "/";

        }


        const locationInput =
            document.getElementById(
                "locationInput"
            );

        const searchButton =
            document.getElementById(
                "locationSearchButton"
            );

        const searchMessage =
            document.getElementById(
                "searchMessage"
            );

        const resultsSection =
            document.getElementById(
                "searchResultsSection"
            );

        const resultsTitle =
            document.getElementById(
                "searchResultsTitle"
            );

        const searchLoading =
            document.getElementById(
                "searchLoading"
            );

        const closeSearchResults =
            document.getElementById(
                "closeSearchResults"
            );


        // =====================================================
        // NORMAL HOME CONTENT CONTROL
        // =====================================================

        function setNormalHomeSectionsVisible(
            visible
        ) {

            const normalSections =
                document.querySelectorAll(
                    "#workHubHome .home-section, " +
                    "#workHubHome .home-cta"
                );


            normalSections.forEach(
                function (section) {

                    section.style.display =
                        visible
                            ? ""
                            : "none";

                }
            );

        }


        // =====================================================
        // SEARCH SUBMIT
        // =====================================================

        searchForm.addEventListener(
            "submit",
            async function (event) {

                event.preventDefault();


                const location =
                    locationInput
                        .value
                        .trim();


                if (!location) {

                    searchMessage.textContent =
                        "Please enter your location.";

                    locationInput.focus();

                    return;

                }


                searchMessage.textContent =
                    "";


                searchButton.disabled =
                    true;


                searchButton.textContent =
                    "Searching...";


                // ---------------------------------------------
                // HIDE NORMAL HOME CONTENT
                // ---------------------------------------------

                setNormalHomeSectionsVisible(
                    false
                );


                // ---------------------------------------------
                // SHOW SEARCH RESULTS
                // ---------------------------------------------

                resultsSection.classList.add(
                    "visible"
                );


                searchLoading.style.display =
                    "block";


                clearSearchResults();


                try {

                    const endpoint =
                        apiBase +
                        "api/Home/search?location=" +
                        encodeURIComponent(
                            location
                        );


                    const response =
                        await fetch(
                            endpoint,
                            {
                                method: "GET",
                                cache: "no-store"
                            }
                        );


                    if (!response.ok) {

                        let errorMessage =
                            "Unable to search WorkHub.";


                        try {

                            const errorData =
                                await response.json();


                            if (errorData.message) {

                                errorMessage =
                                    errorData.message;

                            }

                        }
                        catch {
                        }


                        throw new Error(
                            errorMessage
                        );

                    }


                    const data =
                        await response.json();


                    resultsTitle.textContent =
                        "Available in " +
                        (
                            data.searchedLocation
                            ||
                            location
                        );


                    renderSearchJobs(
                        data.jobs || []
                    );


                    renderSearchServices(
                        data.services || []
                    );


                    renderSearchMarketplace(
                        data.marketplace || []
                    );


                    const totalResults =
                        Number(
                            data.totalResults
                            || 0
                        );


                    if (totalResults === 0) {

                        searchMessage.textContent =
                            "No WorkHub opportunities found in " +
                            location +
                            ".";

                    }


                    resultsSection.scrollIntoView(
                        {
                            behavior: "smooth",
                            block: "start"
                        }
                    );

                }
                catch (error) {

                    console.error(
                        "WorkHub Search Error:",
                        error
                    );


                    searchMessage.textContent =
                        error.message
                        ||
                        "Unable to connect to WorkHub API.";


                    resultsSection.classList.remove(
                        "visible"
                    );


                    // Search fail unoth normal home eka aye pennanawa.
                    setNormalHomeSectionsVisible(
                        true
                    );

                }
                finally {

                    searchLoading.style.display =
                        "none";


                    searchButton.disabled =
                        false;


                    searchButton.textContent =
                        "Search";

                }

            }
        );


        // =====================================================
        // CLOSE SEARCH RESULTS
        // =====================================================

        if (closeSearchResults) {

            closeSearchResults.addEventListener(
                "click",
                function () {

                    resultsSection.classList.remove(
                        "visible"
                    );


                    searchMessage.textContent =
                        "";


                    // -----------------------------------------
                    // SHOW NORMAL HOME CONTENT AGAIN
                    // -----------------------------------------

                    setNormalHomeSectionsVisible(
                        true
                    );


                    clearSearchResults();

                }
            );

        }


        // =====================================================
        // JOB SEARCH RESULTS
        // =====================================================

        function renderSearchJobs(jobs) {

            const container =
                document.getElementById(
                    "jobResults"
                );

            const counter =
                document.getElementById(
                    "jobResultCount"
                );


            if (
                !container ||
                !counter
            ) {
                return;
            }


            counter.textContent =
                jobs.length;


            if (jobs.length === 0) {

                container.innerHTML =
                    createEmptyResult(
                        "No jobs found in this location."
                    );

                return;

            }


            container.innerHTML =
                jobs.map(
                    function (job) {

                        const companyName =
                            job.company
                                ?.companyName
                            ||
                            "WorkHub Company";


                        const category =
                            job.category
                            ||
                            "Job";


                        const location =
                            job.location
                            ||
                            "Location not specified";


                        const jobType =
                            job.jobType
                            ||
                            "Job";


                        return `
                            <article class="workhub-search-card">

                                <div class="workhub-search-card-top job">

                                    <div class="workhub-search-icon">

                                        <svg
                                            viewBox="0 0 24 24"
                                            width="25"
                                            height="25"
                                            fill="none"
                                            stroke="currentColor"
                                            stroke-width="2"
                                            stroke-linecap="round"
                                            stroke-linejoin="round">

                                            <rect
                                                x="3"
                                                y="7"
                                                width="18"
                                                height="13"
                                                rx="2">
                                            </rect>

                                            <path
                                                d="M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2">
                                            </path>

                                            <path
                                                d="M3 12h18">
                                            </path>

                                        </svg>

                                    </div>


                                    <div class="workhub-search-card-type">
                                        Job Opportunity
                                    </div>

                                </div>


                                <div class="workhub-search-card-body">

                                    <span class="workhub-search-badge">

                                        ${escapeHtml(
                            category
                        )}

                                    </span>


                                    <h3 class="workhub-search-card-title">

                                        ${escapeHtml(
                            job.title
                        )}

                                    </h3>


                                    <div class="workhub-search-info">


                                        <div class="workhub-search-info-row">

                                            <span class="workhub-search-info-icon">

                                                <svg
                                                    viewBox="0 0 24 24"
                                                    width="16"
                                                    height="16"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    stroke-width="2">

                                                    <path d="M3 21h18"></path>
                                                    <path d="M6 21V5h12v16"></path>
                                                    <path d="M9 9h2"></path>
                                                    <path d="M13 9h2"></path>
                                                    <path d="M9 13h2"></path>
                                                    <path d="M13 13h2"></path>

                                                </svg>

                                            </span>

                                            <span>
                                                ${escapeHtml(
                            companyName
                        )}
                                            </span>

                                        </div>


                                        <div class="workhub-search-info-row">

                                            <span class="workhub-search-info-icon">

                                                <svg
                                                    viewBox="0 0 24 24"
                                                    width="16"
                                                    height="16"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    stroke-width="2">

                                                    <path
                                                        d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0">
                                                    </path>

                                                    <circle
                                                        cx="12"
                                                        cy="10"
                                                        r="2">
                                                    </circle>

                                                </svg>

                                            </span>

                                            <span>
                                                ${escapeHtml(
                            location
                        )}
                                            </span>

                                        </div>


                                        <div class="workhub-search-info-row">

                                            <span class="workhub-search-info-icon">

                                                <svg
                                                    viewBox="0 0 24 24"
                                                    width="16"
                                                    height="16"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    stroke-width="2">

                                                    <circle
                                                        cx="12"
                                                        cy="12"
                                                        r="9">
                                                    </circle>

                                                    <path
                                                        d="M12 7v5l3 2">
                                                    </path>

                                                </svg>

                                            </span>

                                            <span>
                                                ${escapeHtml(
                            jobType
                        )}
                                            </span>

                                        </div>

                                    </div>

                                </div>


                                <div class="workhub-search-card-footer">

                                    <a
                                        href="/Jobs/Details/${encodeURIComponent(
                            job.jobId
                        )}"
                                        class="workhub-search-button">

                                        <span>
                                            View Job
                                        </span>

                                        <span>
                                            →
                                        </span>

                                    </a>

                                </div>

                            </article>
                        `;

                    }
                )
                    .join("");

        }


        // =====================================================
        // SERVICE SEARCH RESULTS
        // =====================================================

        function renderSearchServices(
            services
        ) {

            const container =
                document.getElementById(
                    "serviceResults"
                );

            const counter =
                document.getElementById(
                    "serviceResultCount"
                );


            if (
                !container ||
                !counter
            ) {
                return;
            }


            counter.textContent =
                services.length;


            if (services.length === 0) {

                container.innerHTML =
                    createEmptyResult(
                        "No services found in this location."
                    );

                return;

            }


            container.innerHTML =
                services.map(
                    function (service) {

                        const providerName =
                            service.provider
                                ?.displayName
                            ||
                            "Service Provider";


                        const location =
                            service.location
                            ||
                            service.provider
                                ?.location
                            ||
                            "Location not specified";


                        const category =
                            service.category
                            ||
                            "Service";


                        return `
                            <article class="workhub-search-card">

                                <div class="workhub-search-card-top service">

                                    <div class="workhub-search-icon">

                                        <svg
                                            viewBox="0 0 24 24"
                                            width="25"
                                            height="25"
                                            fill="none"
                                            stroke="currentColor"
                                            stroke-width="2"
                                            stroke-linecap="round"
                                            stroke-linejoin="round">

                                            <path
                                                d="M14.7 6.3a4 4 0 0 0-5.6 5.6L3 18v3h3l6.1-6.1a4 4 0 0 0 5.6-5.6l-3 3-3-3z">
                                            </path>

                                        </svg>

                                    </div>


                                    <div class="workhub-search-card-type">
                                        Professional Service
                                    </div>

                                </div>


                                <div class="workhub-search-card-body">

                                    <span class="workhub-search-badge">

                                        ${escapeHtml(
                            category
                        )}

                                    </span>


                                    <h3 class="workhub-search-card-title">

                                        ${escapeHtml(
                            service.title
                        )}

                                    </h3>


                                    <div class="workhub-search-info">


                                        <div class="workhub-search-info-row">

                                            <span class="workhub-search-info-icon">

                                                <svg
                                                    viewBox="0 0 24 24"
                                                    width="16"
                                                    height="16"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    stroke-width="2">

                                                    <circle
                                                        cx="12"
                                                        cy="8"
                                                        r="4">
                                                    </circle>

                                                    <path
                                                        d="M4 21a8 8 0 0 1 16 0">
                                                    </path>

                                                </svg>

                                            </span>

                                            <span>
                                                ${escapeHtml(
                            providerName
                        )}
                                            </span>

                                        </div>


                                        <div class="workhub-search-info-row">

                                            <span class="workhub-search-info-icon">

                                                <svg
                                                    viewBox="0 0 24 24"
                                                    width="16"
                                                    height="16"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    stroke-width="2">

                                                    <path
                                                        d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0">
                                                    </path>

                                                    <circle
                                                        cx="12"
                                                        cy="10"
                                                        r="2">
                                                    </circle>

                                                </svg>

                                            </span>

                                            <span>
                                                ${escapeHtml(
                            location
                        )}
                                            </span>

                                        </div>

                                    </div>


                                    <div class="workhub-search-price-block">

                                        <span>
                                            Starting from
                                        </span>

                                        <strong>

                                            ${formatMoney(
                            service.startingPrice
                        )}

                                        </strong>

                                    </div>

                                </div>


                                <div class="workhub-search-card-footer">

                                    <a
                                        href="/Services/Details/${encodeURIComponent(
                            service.serviceOfferingId
                        )}"
                                        class="workhub-search-button">

                                        <span>
                                            View Service
                                        </span>

                                        <span>
                                            →
                                        </span>

                                    </a>

                                </div>

                            </article>
                        `;

                    }
                )
                    .join("");

        }


        // =====================================================
        // MARKETPLACE SEARCH RESULTS
        // =====================================================

        function renderSearchMarketplace(
            listings
        ) {

            const container =
                document.getElementById(
                    "marketplaceResults"
                );

            const counter =
                document.getElementById(
                    "marketplaceResultCount"
                );


            if (
                !container ||
                !counter
            ) {
                return;
            }


            counter.textContent =
                listings.length;


            if (listings.length === 0) {

                container.innerHTML =
                    createEmptyResult(
                        "No marketplace items found in this location."
                    );

                return;

            }


            container.innerHTML =
                listings.map(
                    function (listing) {

                        const listingId =
                            listing.marketplaceListingId
                            ||
                            listing.id;


                        const primaryImageId =
                            listing.primaryImageId
                            ||
                            listing.PrimaryImageId;


                        const category =
                            listing.category
                            ||
                            "Marketplace";


                        const location =
                            listing.location
                            ||
                            "Location not specified";


                        const listingType =
                            listing.listingType
                            ||
                            "Sale";


                        const condition =
                            listing.condition
                            ||
                            "";


                        let imageHtml =
                            "";


                        if (primaryImageId) {

                            imageHtml =
                                `
                                <img
                                    src="${apiBase}api/MarketplaceImages/${encodeURIComponent(
                                    primaryImageId
                                )}/file"
                                    alt="${escapeHtml(
                                    listing.title
                                )}"
                                    class="workhub-marketplace-result-image"
                                    loading="lazy"
                                    onerror="this.style.display='none';"
                                />
                                `;

                        }


                        return `
                            <article class="workhub-search-card marketplace-card">

                                <div class="workhub-marketplace-image-wrap">

                                    ${imageHtml}


                                    <div class="workhub-marketplace-image-placeholder">

                                        <div class="workhub-marketplace-placeholder-icon">

                                            <svg
                                                viewBox="0 0 24 24"
                                                width="27"
                                                height="27"
                                                fill="none"
                                                stroke="currentColor"
                                                stroke-width="2">

                                                <rect
                                                    x="3"
                                                    y="5"
                                                    width="18"
                                                    height="16"
                                                    rx="2">
                                                </rect>

                                                <circle
                                                    cx="8.5"
                                                    cy="10"
                                                    r="2">
                                                </circle>

                                                <path
                                                    d="m21 15-5-5L5 21">
                                                </path>

                                            </svg>

                                        </div>

                                        <span>
                                            Image unavailable
                                        </span>

                                    </div>


                                    <span class="workhub-marketplace-type-badge">

                                        ${escapeHtml(
                            listingType
                        )}

                                    </span>

                                </div>


                                <div class="workhub-search-card-body">

                                    <span class="workhub-search-badge">

                                        ${escapeHtml(
                            category
                        )}

                                    </span>


                                    <h3 class="workhub-search-card-title">

                                        ${escapeHtml(
                            listing.title
                        )}

                                    </h3>


                                    <div class="workhub-marketplace-price">

                                        ${formatMoney(
                            listing.price
                        )}

                                    </div>


                                    <div class="workhub-search-info">


                                        <div class="workhub-search-info-row">

                                            <span class="workhub-search-info-icon">

                                                <svg
                                                    viewBox="0 0 24 24"
                                                    width="16"
                                                    height="16"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    stroke-width="2">

                                                    <path
                                                        d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0">
                                                    </path>

                                                    <circle
                                                        cx="12"
                                                        cy="10"
                                                        r="2">
                                                    </circle>

                                                </svg>

                                            </span>

                                            <span>
                                                ${escapeHtml(
                            location
                        )}
                                            </span>

                                        </div>


                                        ${condition
                                ?
                                `
                                                <div class="workhub-search-info-row">

                                                    <span class="workhub-search-info-icon">

                                                        <svg
                                                            viewBox="0 0 24 24"
                                                            width="16"
                                                            height="16"
                                                            fill="none"
                                                            stroke="currentColor"
                                                            stroke-width="2">

                                                            <path
                                                                d="m12 3 1.7 4.3L18 9l-4.3 1.7L12 15l-1.7-4.3L6 9l4.3-1.7z">
                                                            </path>

                                                        </svg>

                                                    </span>

                                                    <span>
                                                        ${escapeHtml(
                                    condition
                                )}
                                                    </span>

                                                </div>
                                                `
                                :
                                ""
                            }

                                    </div>

                                </div>


                                <div class="workhub-search-card-footer">

                                    <a
                                        href="/Marketplace/Details/${encodeURIComponent(
                                listingId
                            )}"
                                        class="workhub-search-button">

                                        <span>
                                            View Item
                                        </span>

                                        <span>
                                            →
                                        </span>

                                    </a>

                                </div>

                            </article>
                        `;

                    }
                )
                    .join("");

        }


        // =====================================================
        // CLEAR SEARCH RESULTS
        // =====================================================

        function clearSearchResults() {

            const containers =
                [
                    "jobResults",
                    "serviceResults",
                    "marketplaceResults"
                ];


            containers.forEach(
                function (id) {

                    const element =
                        document.getElementById(
                            id
                        );


                    if (element) {

                        element.innerHTML =
                            "";

                    }

                }
            );


            const jobCount =
                document.getElementById(
                    "jobResultCount"
                );

            const serviceCount =
                document.getElementById(
                    "serviceResultCount"
                );

            const marketplaceCount =
                document.getElementById(
                    "marketplaceResultCount"
                );


            if (jobCount) {
                jobCount.textContent = "0";
            }


            if (serviceCount) {
                serviceCount.textContent = "0";
            }


            if (marketplaceCount) {
                marketplaceCount.textContent = "0";
            }

        }


        // =====================================================
        // EMPTY RESULT
        // =====================================================

        function createEmptyResult(
            message
        ) {

            return `
                <div class="workhub-search-empty">

                    <div class="workhub-search-empty-icon">

                        <svg
                            viewBox="0 0 24 24"
                            width="21"
                            height="21"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2">

                            <circle
                                cx="11"
                                cy="11"
                                r="7">
                            </circle>

                            <path
                                d="m20 20-3.5-3.5">
                            </path>

                        </svg>

                    </div>

                    <strong>
                        Nothing available here yet
                    </strong>

                    <span>
                        ${escapeHtml(
                message
            )}
                    </span>

                </div>
            `;

        }


        // =====================================================
        // FORMAT MONEY
        // =====================================================

        function formatMoney(value) {

            if (
                value === null ||
                value === undefined ||
                value === ""
            ) {

                return "Price not specified";

            }


            const numericValue =
                Number(value);


            if (!Number.isFinite(numericValue)) {

                return "Price not specified";

            }


            return new Intl.NumberFormat(
                "en-LK",
                {
                    style: "currency",
                    currency: "LKR",
                    maximumFractionDigits: 0
                }
            )
                .format(
                    numericValue
                );

        }


        // =====================================================
        // ESCAPE HTML
        // =====================================================

        function escapeHtml(value) {

            const div =
                document.createElement(
                    "div"
                );


            div.textContent =
                value ?? "";


            return div.innerHTML;

        }

    }
);