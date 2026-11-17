using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Helpers
{
    public sealed record EmbedCount(int Views, int Visitors);

    public sealed record EmbedSite(int Id, string Host, bool Active);

    /// <summary>
    /// Page view and visitor counts for public embeds (blog view counters, badges).
    /// Results are cached for a few minutes so a busy page doesn't query the database on every load.
    /// </summary>
    public sealed class EmbedStats(VisitTrackerDBContext db, IMemoryCache cache)
    {
        public static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(5);
        public static readonly string[] Periods = ["all", "30d"];

        public EmbedSite? GetSite(int id) =>
            cache.GetOrCreate($"embed:site:{id}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var website = new WebsiteManager(db).GetWebsiteByID(id);
                return website == null ? null : new EmbedSite(website.ID, website.Name, website.Status == RecordStatus.Active);
            });

        /// <param name="requestedPath">"/blog/post", "/blog/post?x=1" or a full URL on the site.</param>
        /// <param name="period">"all" or "30d".</param>
        public EmbedCount Count(EmbedSite site, string requestedPath, string period)
        {
            if (!TryNormalize(site.Host, requestedPath, out var path, out var queryKey))
            {
                return new EmbedCount(0, 0);
            }

            return cache.GetOrCreate($"embed:count:{site.Id}:{period}:{path}{queryKey}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheFor;
                return Query(site.Id, path, queryKey, period);
            })!;
        }

        private EmbedCount Query(int siteId, string path, string queryKey, string period)
        {
            var pageViews = db.VisitPages.AsNoTracking()
                .Where(vp => EF.Property<int>(vp.visit, "WebsiteID") == siteId
                    && vp.webpage.Path == path
                    && !vp.visit.IsBot);

            if (period == "30d")
            {
                var since = DateTime.UtcNow.AddDays(-30);
                pageViews = pageViews.Where(vp => vp.DateCreated >= since);
            }

            // One row per (query-string variant, visitor). Variants that only differ by
            // tracking parameters (gclid, utm_…) are merged below, like the report does.
            var rows = pageViews
                .GroupBy(vp => new { vp.webpage.QueryString, Visitor = vp.visit.VisitorId ?? vp.visit.ClientCookie })
                .Select(g => new { g.Key.QueryString, g.Key.Visitor, Views = g.Count() })
                .ToList();

            var matching = rows
                .Where(r => VisitAnalytics.StripTracking(r.QueryString).Equals(queryKey, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new EmbedCount(matching.Sum(r => r.Views), matching.Select(r => r.Visitor).Distinct().Count());
        }

        private static bool TryNormalize(string host, string requestedPath, out string path, out string queryKey)
        {
            path = string.Empty;
            queryKey = string.Empty;

            var raw = (requestedPath ?? string.Empty).Trim();
            if (raw.Length == 0 || raw.Length > 1024) return false;

            var full = raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? raw
                : "https://" + host + (raw.StartsWith('/') ? raw : "/" + raw);

            if (!UrlNormalizer.TryNormalize(full, out var normalized)) return false;

            path = normalized.Path;
            queryKey = VisitAnalytics.StripTracking(normalized.QueryString);
            return true;
        }

        /// <summary>True when the request comes from the site itself or one of its subdomains.</summary>
        public static bool IsSiteHost(string siteHost, string requestHost)
        {
            var site = VisitAnalytics.StripWww(siteHost.Trim().ToLowerInvariant());
            var host = VisitAnalytics.StripWww(requestHost.Trim().ToLowerInvariant());
            return host == site || host.EndsWith("." + site, StringComparison.Ordinal);
        }
    }
}
