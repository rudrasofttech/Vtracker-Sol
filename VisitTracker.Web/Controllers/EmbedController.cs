using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using VisitTracker.DataContext;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Controllers
{
    /// <summary>
    /// Public page counters for third-party sites.
    ///   GET /api/stats/views?site=1&amp;path=/a&amp;path=/b&amp;period=all&amp;period=30d   → JSON (used by /embed/views.js)
    ///   GET /embed/views.svg?site=1&amp;path=/a&amp;metric=views&amp;period=all          → badge image
    /// </summary>
    public class EmbedController(VisitTrackerDBContext db, IMemoryCache cache, ILogger<EmbedController> logger) : Controller
    {
        private const int MaxPathsPerRequest = 100;
        private static readonly Regex HexColor = new("^[0-9a-fA-F]{6}$", RegexOptions.Compiled);

        [HttpGet("api/stats/views")]
        [EnableRateLimiting("rv-policy")]
        public IActionResult Views(
            [FromQuery] int site,
            [FromQuery(Name = "path")] string[]? paths,
            [FromQuery(Name = "period")] string[]? periods)
        {
            try
            {
                var stats = new EmbedStats(db, cache);
                var info = stats.GetSite(site);
                if (info == null || !info.Active) return NotFound();

                if (!AllowOrigin(info.Host)) return StatusCode(StatusCodes.Status403Forbidden);

                var wantedPeriods = (periods ?? [])
                    .Select(p => p.Trim().ToLowerInvariant())
                    .Where(p => EmbedStats.Periods.Contains(p))
                    .Distinct()
                    .DefaultIfEmpty("all")
                    .ToList();

                var wantedPaths = (paths ?? [])
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct()
                    .Take(MaxPathsPerRequest)
                    .ToList();

                // Keys are the paths exactly as requested, so the script can match them back to placeholders.
                var result = new Dictionary<string, Dictionary<string, EmbedCount>>();
                foreach (var path in wantedPaths)
                {
                    result[path] = wantedPeriods.ToDictionary(p => p, p => stats.Count(info, path, p));
                }

                Response.Headers.CacheControl = "public, max-age=300";
                return Json(new { site, stats = result });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Embed > Views");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("embed/views.svg")]
        public IActionResult Badge(
            [FromQuery] int site,
            [FromQuery] string? path,
            [FromQuery] string? metric,
            [FromQuery] string? period,
            [FromQuery] string? label,
            [FromQuery] string? color)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return BadRequest("path is required");

                var stats = new EmbedStats(db, cache);
                var info = stats.GetSite(site);
                if (info == null || !info.Active) return NotFound();

                var visitors = string.Equals(metric, "visitors", StringComparison.OrdinalIgnoreCase);
                var p = string.Equals(period, "30d", StringComparison.OrdinalIgnoreCase) ? "30d" : "all";
                var count = stats.Count(info, path, p);
                var value = (visitors ? count.Visitors : count.Views).ToString("N0", CultureInfo.InvariantCulture);

                var text = string.IsNullOrWhiteSpace(label) ? (visitors ? "visitors" : "views") : label.Trim();
                if (text.Length > 30) text = text[..30];
                var fill = !string.IsNullOrEmpty(color) && HexColor.IsMatch(color) ? "#" + color : "#e74c3c";

                Response.Headers.CacheControl = "public, max-age=300";
                return Content(BadgeSvg(text, value, fill), "image/svg+xml; charset=utf-8");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Embed > Badge");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Browsers send Origin on cross-site fetch. Only the website's own pages (and subdomains) may read
        /// its counts from the browser. Requests without Origin (server-side or same-origin) are allowed.
        /// </summary>
        private bool AllowOrigin(string siteHost)
        {
            var origin = Request.Headers.Origin.ToString();
            if (string.IsNullOrEmpty(origin)) return true;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || !EmbedStats.IsSiteHost(siteHost, uri.Host))
            {
                return false;
            }

            Response.Headers.AccessControlAllowOrigin = origin;
            Response.Headers.Vary = "Origin";
            return true;
        }

        private static string BadgeSvg(string label, string value, string fill)
        {
            // Approximate text widths for 11px Verdana; good enough for short labels and numbers.
            static int Width(string s) => (int)Math.Ceiling(s.Length * 6.8) + 12;

            var lw = Width(label);
            var vw = Width(value);
            var total = lw + vw;
            var l = WebUtility.HtmlEncode(label);
            var v = WebUtility.HtmlEncode(value);

            return string.Create(CultureInfo.InvariantCulture, $"""
                <svg xmlns="http://www.w3.org/2000/svg" width="{total}" height="20" role="img" aria-label="{l}: {v}">
                  <title>{l}: {v}</title>
                  <clipPath id="r"><rect width="{total}" height="20" rx="3"/></clipPath>
                  <g clip-path="url(#r)">
                    <rect width="{lw}" height="20" fill="#555"/>
                    <rect x="{lw}" width="{vw}" height="20" fill="{fill}"/>
                  </g>
                  <g fill="#fff" text-anchor="middle" font-family="Verdana,DejaVu Sans,sans-serif" font-size="11">
                    <text x="{lw / 2.0}" y="14">{l}</text>
                    <text x="{lw + vw / 2.0}" y="14">{v}</text>
                  </g>
                </svg>
                """);
        }
    }
}
