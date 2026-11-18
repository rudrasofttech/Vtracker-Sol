/*!
 * WebStats view counter embed.
 *
 *   <script src="https://www.webstats.co.in/embed/views.js" data-site="1" defer></script>
 *
 *   <span data-webstats="views"></span>                        views of this page, all time
 *   <span data-webstats="visitors" data-period="30d"></span>   unique visitors, last 30 days
 *   <span data-webstats="views" data-path="/blog/my-post"></span>
 *
 * Options on each placeholder:
 *   data-webstats  "views" (default) or "visitors"
 *   data-path      page to count; defaults to the current page
 *   data-period    "all" (default) or "30d"
 *   data-format    "short" for 1.2K instead of 1,234
 *   data-label     text template, e.g. "Read {n} times"; default "{n} views" / "{n} visitors"
 *
 * Call WebStatsEmbed.refresh() after adding placeholders dynamically (infinite scroll, SPA navigation).
 * Each filled placeholder gets data-webstats-value="<number>", and document fires "webstats:updated"
 * after every batch, so host pages can sort or highlight by count.
 */
(function () {
    'use strict';

    var script = document.currentScript || document.querySelector('script[src*="/embed/views.js"]');
    if (!script) return;

    var site = script.getAttribute('data-site');
    if (!site) return;

    var base;
    try { base = new URL(script.src, location.href).origin; } catch (e) { return; }

    var SELECTOR = '[data-webstats],[data-webstats-views],[data-webstats-visitors]';
    // IIS rejects query strings over 2048 characters by default, so requests are split by length.
    var MAX_URL = 1800;
    var MAX_PATHS = 50;

    function metricOf(el) {
        var m = (el.getAttribute('data-webstats') || '').toLowerCase();
        return (m === 'visitors' || el.hasAttribute('data-webstats-visitors')) ? 'visitors' : 'views';
    }

    function periodOf(el) {
        return (el.getAttribute('data-period') || '').toLowerCase() === '30d' ? '30d' : 'all';
    }

    function pathOf(el) {
        return el.getAttribute('data-path') || (location.pathname + location.search);
    }

    function format(n, el) {
        try {
            var opts = el.getAttribute('data-format') === 'short'
                ? { notation: 'compact', maximumFractionDigits: 1 }
                : {};
            return new Intl.NumberFormat(undefined, opts).format(n);
        } catch (e) {
            return String(n);
        }
    }

    function text(n, el, metric) {
        var value = format(n, el);
        var template = el.getAttribute('data-label');
        if (template) return template.split('{n}').join(value);
        var word = metric === 'visitors' ? (n === 1 ? 'visitor' : 'visitors') : (n === 1 ? 'view' : 'views');
        return value + ' ' + word;
    }

    function unique(list) {
        var seen = {}, out = [];
        for (var i = 0; i < list.length; i++) {
            if (!seen[list[i]]) { seen[list[i]] = true; out.push(list[i]); }
        }
        return out;
    }

    function fill(elements, stats) {
        elements.forEach(function (el) {
            var byPeriod = stats[pathOf(el)];
            var count = byPeriod && byPeriod[periodOf(el)];
            if (!count) return;
            var metric = metricOf(el);
            el.textContent = text(count[metric] || 0, el, metric);
            el.setAttribute('data-webstats-value', String(count[metric] || 0));
            el.setAttribute('data-webstats-done', '1');
        });
    }

    function refresh(root) {
        var elements = Array.prototype.slice
            .call((root || document).querySelectorAll(SELECTOR))
            .filter(function (el) { return !el.hasAttribute('data-webstats-done'); });

        if (elements.length === 0) return Promise.resolve();

        var paths = unique(elements.map(pathOf));
        var periods = unique(elements.map(periodOf));
        var requests = [];

        var prefix = base + '/api/stats/views?site=' + encodeURIComponent(site)
            + periods.map(function (p) { return '&period=' + p; }).join('');

        function send(query) {
            requests.push(
                fetch(prefix + query, { credentials: 'omit' })
                    .then(function (r) { return r.ok ? r.json() : null; })
                    .then(function (data) { if (data && data.stats) fill(elements, data.stats); })
                    .catch(function () { /* leave the placeholder's own text in place */ })
            );
        }

        var query = '', count = 0;
        paths.forEach(function (p) {
            var part = '&path=' + encodeURIComponent(p);
            if (count > 0 && (prefix.length + query.length + part.length > MAX_URL || count >= MAX_PATHS)) {
                send(query);
                query = '';
                count = 0;
            }
            query += part;
            count++;
        });
        if (count > 0) send(query);

        return Promise.all(requests).then(function () {
            // Lets the host page react once counts are in (e.g. sort a table by views).
            try { document.dispatchEvent(new CustomEvent('webstats:updated')); } catch (e) { }
        });
    }

    var initial = null;
    function start() { initial = refresh(); }

    window.WebStatsEmbed = {
        refresh: refresh,
        /** Promise that resolves when the first batch of counts has been filled in. */
        ready: function () { return initial || Promise.resolve(); }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }
})();