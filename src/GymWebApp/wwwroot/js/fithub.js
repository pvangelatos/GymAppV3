// wwwroot/js/fithub.js — app shell: collapsible sidebar (persisted), mobile drawer, tooltips.
(function () {
    "use strict";
    var KEY = "fithub.sidebar.collapsed";
    var root = document.documentElement;
    var shell = document.getElementById("fhShell");
    if (!shell) return; // public pages have no sidebar

    var tooltips = [];
    function refreshTooltips() {
        tooltips.forEach(function (t) { t.dispose(); });
        tooltips = [];
        if (!root.classList.contains("fh-collapsed")) return; // labels are visible when expanded
        document.querySelectorAll('.fh-sidebar [data-bs-toggle="tooltip"]').forEach(function (el) {
            tooltips.push(new bootstrap.Tooltip(el, { trigger: "hover focus" }));
        });
    }

    function setCollapsed(on) {
        root.classList.toggle("fh-collapsed", on);
        try { localStorage.setItem(KEY, on ? "1" : "0"); } catch (e) { }
        var btn = document.querySelector('[data-fh-toggle="collapse"]');
        if (btn) btn.setAttribute("aria-expanded", String(!on));
        refreshTooltips();
    }

    // The <head> script already applied the saved state; sync the button and tooltips with it.
    setCollapsed(root.classList.contains("fh-collapsed"));

    document.addEventListener("click", function (e) {
        var toggle = e.target.closest("[data-fh-toggle]");
        if (toggle) {
            var mode = toggle.getAttribute("data-fh-toggle");
            if (mode === "collapse") setCollapsed(!root.classList.contains("fh-collapsed"));
            if (mode === "mobile") shell.classList.toggle("is-mobile-open");
            return;
        }
        if (e.target.closest(".fh-nav-link")) shell.classList.remove("is-mobile-open");
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") shell.classList.remove("is-mobile-open");
    });
})();