using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Pages
{
    /// <summary>A website shown in the public "Websites using WebStats" list. No traffic numbers are exposed.</summary>
    public sealed record ShowcaseSite(int Id, string Host, int SinceYear, bool ActiveThisWeek);

    public sealed record LandingData(
        int VisitsLast24h,
        int[] Hourly,
        string[] HourLabels,
        int TotalVisits,
        int FirstYear,
        List<ShowcaseSite> Sites,
        int? DemoSiteId,
        string? DemoHost)
    {
        public static readonly LandingData Empty = new(0, new int[24], new string[24], 0, DateTime.UtcNow.Year, [], null, null);
    }

    /// <summary>Public home page: explains WebStats to people who might want to use it.</summary>
    public class IndexModel(ILogger<IndexModel> logger, VisitTrackerDBContext db, IMemoryCache cache) : PageModel
    {
        /// <summary>The website whose report is used as the public live demo (falls back to the busiest listed site).</summary>
        public const string DemoSiteName = "rudrasofttech.com";

        /// <summary>Where "Add your website" goes until self-service sign-up exists.</summary>
        public const string ContactUrl = "https://www.rudrasofttech.com";

        /// <summary>
        /// The tracking code a site owner pastes, with {id} for their website ID.
        /// Leave empty to show a sentence instead of a code sample.
        /// </summary>
        public const string TrackingSnippet = "";

        public LandingData Data { get; private set; } = LandingData.Empty;

        public void OnGet()
        {
            try
            {
                Data = cache.GetOrCreate("landing:v1", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return Load();
                }) ?? LandingData.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Index > OnGet");
            }
        }

        private LandingData Load()
        {
            var now = DateTime.UtcNow;
            var since24h = now.AddHours(-24);
            var since7d = now.AddDays(-7);
            var since30d = now.AddDays(-30);

            var websites = db.Set<Website>().AsNoTracking()
                .Where(w => w.Status == RecordStatus.Active)
                .Select(w => new { w.ID, w.Name })
                .ToList();

            // One grouped query for all sites: total, first and last human visit.
            var perSite = db.Visits.AsNoTracking()
                .Where(v => !v.IsBot)
                .GroupBy(v => EF.Property<int>(v, "WebsiteID"))
                .Select(g => new
                {
                    Id = g.Key,
                    Total = g.Count(),
                    First = g.Min(v => v.DateCreated),
                    Last = g.Max(v => v.DateCreated)
                })
                .ToDictionary(x => x.Id);

            // Only sites that had visits in the last 30 days count as "using WebStats".
            var listed = websites
                .Where(w => perSite.TryGetValue(w.ID, out var s) && s.Last >= since30d)
                .OrderByDescending(w => perSite[w.ID].Total)
                .ToList();

            var sites = listed
                .Select(w => new ShowcaseSite(
                    w.ID,
                    w.Name.Trim().ToLowerInvariant(),
                    perSite[w.ID].First.Year,
                    perSite[w.ID].Last >= since7d))
                .ToList();

            var activeIds = websites.Select(w => w.ID).ToHashSet();
            var totalVisits = perSite.Values.Where(s => activeIds.Contains(s.Id)).Sum(s => s.Total);
            var firstYear = perSite.Values.Where(s => activeIds.Contains(s.Id)).Select(s => s.First.Year).DefaultIfEmpty(now.Year).Min();

            // Last 24 hours, hour by hour, across every site.
            var recent = db.Visits.AsNoTracking()
                .Where(v => !v.IsBot && v.DateCreated >= since24h)
                .Select(v => v.DateCreated)
                .ToList();

            var hourly = new int[24];
            foreach (var created in recent)
            {
                var i = (int)Math.Floor((created - since24h).TotalHours);
                hourly[Math.Clamp(i, 0, 23)]++;
            }

            var ist = ResolveIndiaTime();
            var labels = Enumerable.Range(0, 24)
                .Select(i =>
                {
                    var start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(since24h.AddHours(i), DateTimeKind.Utc), ist);
                    return start.ToString("h tt");
                })
                .ToArray();

            var demo = listed.FirstOrDefault(w => w.Name.Trim().Equals(DemoSiteName, StringComparison.OrdinalIgnoreCase))
                ?? listed.FirstOrDefault();

            return new LandingData(
                VisitsLast24h: recent.Count,
                Hourly: hourly,
                HourLabels: labels,
                TotalVisits: totalVisits,
                FirstYear: firstYear,
                Sites: sites,
                DemoSiteId: demo?.ID,
                DemoHost: demo?.Name.Trim().ToLowerInvariant());
        }

        private static TimeZoneInfo ResolveIndiaTime()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); }
            catch { return TimeZoneInfo.Utc; }
        }
    }
}
