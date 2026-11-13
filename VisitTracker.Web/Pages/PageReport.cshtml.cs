using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VisitTracker.DataContext;
using VisitTracker.Models;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Pages
{
    public class PageReportModel(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment) : PageModel
    {
        private readonly VisitTrackerDBContext context = _context;
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);

        [FromQuery(Name = "id")]
        public int Id { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? Start { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? End { get; set; }

        [FromQuery(Name = "path")]
        public string Path { get; set; }

        [FromQuery(Name = "bots")]
        public int? Bots { get; set; }

        public bool IncludeBots => Bots == 1;
        public int HiddenBotVisits { get; set; }

        public WebpageDisplayPublic DisplayInfo { get; set; } = new();

        public void OnGet()
        {
            var website = websiteRepository.GetWebsiteByID(Id);
            if (website == null)
            {
                return;
            }

            var tz = ResolveTimeZone(website.TimeZone);
            var localStart = Start?.Date ?? DateTime.UtcNow.Date.AddDays(-15);
            var localEndDate = End?.Date ?? DateTime.UtcNow.Date;
            var localEndExclusive = localEndDate.AddDays(1);

            var utcStart = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified), tz);
            var utcEndExclusive = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localEndExclusive, DateTimeKind.Unspecified), tz);

            DisplayInfo.Path = Path;
            DisplayInfo.WebsiteId = Id;
            DisplayInfo.Start = localStart;
            DisplayInfo.End = localEndExclusive.AddSeconds(-1);
            DisplayInfo.WebsiteName = website.Name;
            DisplayInfo.VisitChartDescription = $"Visits from {DisplayInfo.Start:d/MMM/yy} till {DisplayInfo.End}";

            if (UrlNormalizer.TryNormalize(Path, out var normalized) == false)
            {
                return;
            }

            var webpage = webpageRepository.GetWebpage(website.ID, normalized.Path, normalized.QueryString);
            if (webpage == null)
            {
                return;
            }

            var pageVisitsQuery = context.VisitPages.AsNoTracking()
                .Where(vp =>
                    vp.webpage.ID == webpage.ID &&
                    vp.DateCreated >= utcStart &&
                    vp.DateCreated < utcEndExclusive &&
                    (IncludeBots || !vp.visit.IsBot))
                .Select(vp => vp.visit.ID)
                .Distinct();

            var baseVisits = context.Visits.AsNoTracking()
                .Where(v => pageVisitsQuery.Contains(v.ID));

            var allIncludingBots = context.VisitPages.AsNoTracking()
                .Where(vp => vp.webpage.ID == webpage.ID && vp.DateCreated >= utcStart && vp.DateCreated < utcEndExclusive)
                .Select(vp => vp.visit.ID)
                .Distinct();

            var botsInRange = context.Visits.AsNoTracking()
                .Where(v => allIncludingBots.Contains(v.ID) && v.IsBot)
                .Count();

            HiddenBotVisits = IncludeBots ? 0 : botsInRange;
            DisplayInfo.VisitCount = baseVisits.Count();

            var hourly = baseVisits
                .GroupBy(v => new { v.DateCreated.Year, v.DateCreated.Month, v.DateCreated.Day, v.DateCreated.Hour })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    g.Key.Hour,
                    All = g.Count(),
                    New = g.Sum(v => v.LastVisitID.HasValue ? 0 : 1),
                    Return = g.Sum(v => v.LastVisitID.HasValue ? 1 : 0)
                })
                .ToList()
                .ToDictionary(
                    x => new DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0, DateTimeKind.Utc),
                    x => (x.All, x.New, x.Return));

            BuildTimeSeries(hourly, tz, localStart, localEndDate);

            var browserRaw = baseVisits
                .GroupBy(v => v.BrowserName ?? string.Empty)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToList();

            DisplayInfo.BrowserData = BuildBrowserChart(browserRaw, x => x.Name, x => x.Count);
        }

        private static TimeZoneInfo ResolveTimeZone(string? id)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
            }
            catch
            {
            }

            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }

        private void BuildTimeSeries(
            Dictionary<DateTime, (int All, int New, int Return)> hourlyUtc,
            TimeZoneInfo tz,
            DateTime localStart,
            DateTime localEndDate)
        {
            var endExclusive = localEndDate.AddDays(1);
            var range = ResolveRange(localStart, localEndDate);
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;

            switch (range)
            {
                case ReportDateRangeCustomType.Day:
                    for (var h = localStart; h < endExclusive; h = h.AddHours(1))
                    {
                        var agg = AggregateHours(hourlyUtc, tz, h, h.AddHours(1));
                        var label = h.ToString("h:mm tt");
                        if (h.Date == todayLocal)
                        {
                            label += " (today partial)";
                        }

                        AddPoint(label, agg);
                    }

                    break;
                case ReportDateRangeCustomType.Days:
                    for (var d = localStart.Date; d < endExclusive; d = d.AddDays(1))
                    {
                        var agg = AggregateHours(hourlyUtc, tz, d, d.AddDays(1));
                        var label = d.ToString("MM/d");
                        if (d.Date == todayLocal)
                        {
                            label += " (today partial)";
                        }

                        AddPoint(label, agg);
                    }

                    break;
                default:
                    for (var d = localStart.Date; d < endExclusive; d = d.AddDays(15))
                    {
                        var end = d.AddDays(15);
                        if (end > endExclusive)
                        {
                            end = endExclusive;
                        }

                        var agg = AggregateHours(hourlyUtc, tz, d, end);
                        var label = d.ToString("yyyy-M-d");
                        if (d <= todayLocal && todayLocal < end)
                        {
                            label += " (today partial)";
                        }

                        AddPoint(label, agg);
                    }

                    break;
            }
        }

        private static (int All, int New, int Return) AggregateHours(
            Dictionary<DateTime, (int All, int New, int Return)> hourlyUtc,
            TimeZoneInfo tz,
            DateTime localStartInclusive,
            DateTime localEndExclusive)
        {
            var all = 0;
            var @new = 0;
            var ret = 0;

            for (var h = localStartInclusive; h < localEndExclusive; h = h.AddHours(1))
            {
                var utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(h, DateTimeKind.Unspecified), tz);
                var key = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);

                if (hourlyUtc.TryGetValue(key, out var bucket))
                {
                    all += bucket.All;
                    @new += bucket.New;
                    ret += bucket.Return;
                }
            }

            return (all, @new, ret);
        }

        private static ReportDateRangeCustomType ResolveRange(DateTime start, DateTime endDate)
        {
            if (start.Date == endDate.Date) return ReportDateRangeCustomType.Day;
            if (endDate.Subtract(start).TotalDays < 60) return ReportDateRangeCustomType.Days;
            return ReportDateRangeCustomType.Months;
        }

        private void AddPoint(string label, (int All, int New, int Return) agg)
        {
            DisplayInfo.VisitsData.Add(new VisitCountChartPoint { X = label, Y = agg.All });
            DisplayInfo.NewVisitsData.Add(new VisitCountChartPoint { X = label, Y = agg.New });
            DisplayInfo.ReturnVisitsData.Add(new VisitCountChartPoint { X = label, Y = agg.Return });
        }

        private static List<PieChartPoint> BuildBrowserChart<T>(
            IEnumerable<T> browserRows,
            Func<T, string> nameSelector,
            Func<T, int> countSelector)
        {
            var counters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Chrome"] = 0,
                ["Firefox"] = 0,
                ["Safari"] = 0,
                ["Opera"] = 0,
                ["Edge"] = 0,
                ["Samsung Internet"] = 0,
                ["Headless Chrome"] = 0,
                ["IE"] = 0,
                ["Other"] = 0
            };

            foreach (var row in browserRows)
            {
                var name = (nameSelector(row) ?? string.Empty).Trim();
                var count = countSelector(row);

                if (name.StartsWith("Headless Chrome", StringComparison.OrdinalIgnoreCase)) counters["Headless Chrome"] += count;
                else if (name.StartsWith("Edge", StringComparison.OrdinalIgnoreCase)) counters["Edge"] += count;
                else if (name.StartsWith("Samsung Internet", StringComparison.OrdinalIgnoreCase)) counters["Samsung Internet"] += count;
                else if (name.StartsWith("Opera", StringComparison.OrdinalIgnoreCase)) counters["Opera"] += count;
                else if (name.StartsWith("Chrome", StringComparison.OrdinalIgnoreCase)) counters["Chrome"] += count;
                else if (name.StartsWith("Safari", StringComparison.OrdinalIgnoreCase)) counters["Safari"] += count;
                else if (name.StartsWith("Firefox", StringComparison.OrdinalIgnoreCase)) counters["Firefox"] += count;
                else if (name.StartsWith("Microsoft Internet Explorer", StringComparison.OrdinalIgnoreCase)) counters["IE"] += count;
                else counters["Other"] += count;
            }

            return counters
                .Select(x => new PieChartPoint { Label = x.Key, Percentage = x.Value })
                .ToList();
        }
    }
}
