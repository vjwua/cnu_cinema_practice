(function () {
    "use strict";

    const queryInput = document.getElementById("external-query");
    const searchBtn = document.getElementById("external-search-btn");
    const resultsDiv = document.getElementById("external-results");
    const previewDiv = document.getElementById("external-preview");
    const messageDiv = document.getElementById("external-message");
    const statusBadge = document.getElementById("external-status");

    if (!queryInput || !searchBtn || !resultsDiv || !previewDiv || !messageDiv || !statusBadge) {
        return;
    }

    const endpoints = {
        check: "/Admin/Movie/ExternalApiStatus",
        search: "/Admin/Movie/SearchExternal",
        preview: (id) => `/Admin/Movie/ImportPreview/${id}`,
        import: "/Admin/Movie/ImportFromExternal"
    };

    function setMessage(html, kind) {
        messageDiv.style.display = "block";
        messageDiv.className = `alert alert-${kind || "secondary"}`;
        messageDiv.innerHTML = html;
    }

    function clearMessage() {
        messageDiv.style.display = "none";
        messageDiv.className = "";
        messageDiv.innerHTML = "";
    }

    function clearResults() {
        resultsDiv.innerHTML = "";
    }

    function clearPreview() {
        previewDiv.innerHTML = "";
    }

    function debounce(fn, wait) {
        let t = null;
        return function (...args) {
            if (t) window.clearTimeout(t);
            t = window.setTimeout(() => fn.apply(this, args), wait);
        };
    }

    async function checkApi() {
        try {
            const resp = await fetch(endpoints.check, { headers: { "X-Requested-With": "XMLHttpRequest" } });
            if (!resp.ok) throw new Error("status");
            const data = await resp.json();
            if (data && data.available) {
                statusBadge.className = "badge text-bg-success";
                statusBadge.textContent = "API available";
            } else {
                statusBadge.className = "badge text-bg-warning";
                statusBadge.textContent = "API unavailable";
                setMessage("External API is unavailable. You can still enter movie data manually.", "warning");
            }
        } catch {
            statusBadge.className = "badge text-bg-warning";
            statusBadge.textContent = "API unavailable";
            setMessage("External API is unavailable. You can still enter movie data manually.", "warning");
        }
    }

    async function doSearch() {
        const q = (queryInput.value || "").trim();

        clearPreview();
        clearResults();
        clearMessage();

        if (q.length < 2) {
            return;
        }

        try {
            const url = `${endpoints.search}?query=${encodeURIComponent(q)}`;
            const resp = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });

            if (resp.status === 429) {
                setMessage("Too many search requests. Please wait a few seconds and try again.", "warning");
                return;
            }

            if (!resp.ok) {
                setMessage("Search failed. You can still enter movie data manually.", "warning");
                return;
            }

            const html = await resp.text();
            resultsDiv.innerHTML = html;
        } catch {
            setMessage("Search failed (network error). You can still enter movie data manually.", "warning");
        }
    }

    const debouncedSearch = debounce(doSearch, 300);

    async function loadPreview(externalId) {
        clearPreview();
        clearMessage();

        try {
            const resp = await fetch(endpoints.preview(externalId), { headers: { "X-Requested-With": "XMLHttpRequest" } });
            if (!resp.ok) {
                setMessage("Failed to load movie details.", "warning");
                return;
            }
            const html = await resp.text();
            previewDiv.innerHTML = html;
        } catch {
            setMessage("Failed to load movie details (network error).", "warning");
        }
    }

    async function importToForm(externalId) {
        clearMessage();

        try {
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenInput ? tokenInput.value : "";

            const resp = await fetch(endpoints.import, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token,
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: JSON.stringify({ externalId })
            });

            if (!resp.ok) {
                setMessage("Import failed. You can still fill the form manually.", "warning");
                return;
            }

            const data = await resp.json();
            if (!data || !data.success) {
                setMessage(data && data.message ? data.message : "Import failed.", "warning");
                return;
            }

            const setVal = (id, val) => {
                const el = document.getElementById(id);
                if (!el) return;
                el.value = val ?? "";
            };

            const movie = data.movie || {};

            setVal("Name", movie.name);
            setVal("Description", movie.description);
            setVal("DurationMinutes", movie.durationMinutes);
            setVal("AgeLimit", movie.ageLimit);
            setVal("GenresString", movie.genresText);
            setVal("PosterUrl", movie.posterUrl);
            setVal("TrailerUrl", movie.trailerUrl);
            setVal("Country", movie.country);
            setVal("Director", movie.director);
            setVal("ImdbRating", movie.imdbRating);
            setVal("ReleaseDate", movie.releaseDate);

            clearPreview();
            clearResults();

            queryInput.value = "";

            setMessage("Movie data imported. Please review and edit before saving.", "success");
        } catch {
            setMessage("Import failed (network error).", "warning");
        }
    }

    queryInput.addEventListener("input", debouncedSearch);
    searchBtn.addEventListener("click", doSearch);

    document.addEventListener("click", function (e) {
        const t = e.target;
        if (!(t instanceof Element)) return;

        const selectBtn = t.closest('[data-action="external-select"]');
        if (selectBtn) {
            const id = parseInt(selectBtn.getAttribute("data-external-id") || "0", 10);
            if (id > 0) {
                clearResults();
                loadPreview(id);
            }
            return;
        }

        const importBtn = t.closest('[data-action="external-import"]');
        if (importBtn) {
            const id = parseInt(importBtn.getAttribute("data-external-id") || "0", 10);
            if (id > 0) {
                clearPreview();
                importToForm(id);
            }
        }
    });

    checkApi();
})();
