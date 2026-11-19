using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Pages
{
    /// <summary>A website on the public home page. Featured sites also carry their numbers and a 30-day trend.</summary>
    public sealed record ShowcaseSite(
        int Id,
        string Host,
        int SinceYear,
        bool ActiveThisWeek,
        int TotalVisits,
        int VisitsLast30Days,
        int[] Daily);

    public sealed record LandingData(
        int VisitsLast24h,
        int[] Hourly,
        string[] HourLabels,
        int TotalVisits,
        int FirstYear,
        List<ShowcaseSite> Featured,
        List<ShowcaseSite> Others)
    {
        public int SiteCount => Featured.Count + Others.Count;
        public static readonly LandingData Empty = new(0, new int[24], new string[24], 0, DateTime.UtcNow.Year, [], []);
    }

    /// <summary>Public home page: explains WebStats to people who might want to use it.</summary>
    public class IndexModel(ILogger<IndexModel> logger, VisitTrackerDBContext db, IMemoryCache cache) : PageModel
    {
        /// <summary>How many of the busiest websites get a featured panel with their numbers.</summary>
        public const int FeaturedCount = 3;

        /// <summary>Where "Add your website" goes until self-service sign-up exists.</summary>
        public const string ContactUrl = "https://www.rudrasofttech.com";

        /// <summary>
        /// The code a site owner pastes, with {id} for their website ID.
        /// Leave empty to describe it in words instead of showing a code sample.
        /// </summary>
        public const string InstallSnippet = "";

        public LandingData Data { get; private set; } = LandingData.Empty;

        public void OnGet()
        {
            try
            {
                Data = cache.GetOrCreate("landing:v2", entry =>
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

            // All-time numbers per site (bots excluded): total, first and last visit.
            var allTime = db.Visits.AsNoTracking()
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

            // Visits per site in the last 30 days, which decides the order.
            var recent = db.Visits.AsNoTracking()
                .Where(v => !v.IsBot && v.DateCreated >= since30d)
                .GroupBy(v => EF.Property<int>(v, "WebsiteID"))
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Id, x => x.Count);

            // A site is listed when it had visits in the last 30 days.
            var listed = websites
                .Where(w => recent.ContainsKey(w.ID) && allTime.ContainsKey(w.ID))
                .OrderByDescending(w => recent[w.ID])
                .ThenByDescending(w => allTime[w.ID].Total)
                .ToList();

            ShowcaseSite ToSite(int id, string name, int[] daily) => new(
                id,
                name.Trim().ToLowerInvariant(),
                allTime[id].First.Year,
                allTime[id].Last >= since7d,
                allTime[id].Total,
                recent[id],
                daily);

            var featured = listed
                .Take(FeaturedCount)
                .Select(w => ToSite(w.ID, w.Name, DailyVisits(w.ID, since30d)))
                .ToList();

            var others = listed
                .Skip(FeaturedCount)
                .Select(w => ToSite(w.ID, w.Name, []))
                .ToList();

            var activeIds = websites.Select(w => w.ID).ToHashSet();
            var totalVisits = allTime.Values.Where(s => activeIds.Contains(s.Id)).Sum(s => s.Total);
            var firstYear = allTime.Values.Where(s => activeIds.Contains(s.Id)).Select(s => s.First.Year).DefaultIfEmpty(now.Year).Min();

            // Last 24 hours, hour by hour, across every site.
            var lastDay = db.Visits.AsNoTracking()
                .Where(v => !v.IsBot && v.DateCreated >= since24h)
                .Select(v => v.DateCreated)
                .ToList();

            var hourly = new int[24];
            foreach (var created in lastDay)
            {
                var i = (int)Math.Floor((created - since24h).TotalHours);
                hourly[Math.Clamp(i, 0, 23)]++;
            }

            var ist = ResolveIndiaTime();
            var labels = Enumerable.Range(0, 24)
                .Select(i => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(since24h.AddHours(i), DateTimeKind.Utc), ist).ToString("h tt"))
                .ToArray();

            return new LandingData(lastDay.Count, hourly, labels, totalVisits, firstYear, featured, others);
        }

        /// <summary>Visits per day for one site over the last 30 days (oldest first).</summary>
        private int[] DailyVisits(int websiteId, DateTime sinceUtc)
        {
            var start = sinceUtc.Date;
            var counts = db.Visits.AsNoTracking()
                .Where(v => !v.IsBot && EF.Property<int>(v, "WebsiteID") == websiteId && v.DateCreated >= start)
                .GroupBy(v => v.DateCreated.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Day, x => x.Count);

            var days = (DateTime.UtcNow.Date - start).Days + 1;
            return Enumerable.Range(0, days).Select(i => counts.GetValueOrDefault(start.AddDays(i))).ToArray();
        }

        private static TimeZoneInfo ResolveIndiaTime()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); }
            catch { return TimeZoneInfo.Utc; }
        }
    }
}
