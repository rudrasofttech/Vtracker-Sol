using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VisitTracker.DataContext;
using VisitTracker.Models;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Pages
{
    public class ReportModel(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment, ILogger<ReportModel> _logger) : PageModel
    {
        private readonly VisitTrackerDBContext context = _context;
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);
        private readonly ILogger<ReportModel> logger = _logger;

        [FromQuery(Name = "id")]
        public int Id { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? Start { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? End { get; set; }

        [FromQuery(Name = "bots")]
        public int? Bots { get; set; }

        public bool IncludeBots => Bots == 1;
        public int HiddenBotVisits { get; set; }

        public WebsiteDisplayPublic DisplayInfo { get; set; } = new(0, DateTime.UtcNow, DateTime.UtcNow);

        public Dictionary<string, List<string>> ReferrerDetails { get; set; } = [];

        public void OnGet()
        {
            try
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

                DisplayInfo = new(Id, localStart, localEndExclusive.AddSeconds(-1))
                {
                    WebsiteName = website.Name
                };

                var visitQuery = context.Visits.AsNoTracking()
                    .Where(v =>
                        EF.Property<int>(v, "WebsiteID") == Id &&
                        v.DateCreated >= utcStart &&
                        v.DateCreated < utcEndExclusive);

                var totalInRange = visitQuery.Count();
                var totalBotsInRange = visitQuery.Count(v => v.IsBot);

                if (!IncludeBots)
                {
                    visitQuery = visitQuery.Where(v => !v.IsBot);
                }

                HiddenBotVisits = IncludeBots ? 0 : totalBotsInRange;
                DisplayInfo.VisitCount = visitQuery.Count();

                // Bucketing source: hourly UTC grouped in SQL
                var hourly = visitQuery
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

                // Browsers
                var browserRaw = visitQuery
                    .GroupBy(v => v.BrowserName ?? string.Empty)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToList();

                DisplayInfo.BrowserData = BuildBrowserChart(browserRaw, x => x.Name, x => x.Count);

                // Screen sizes
                DisplayInfo.ScreenSizes = visitQuery
                    .Where(v => v.ScreenWidth.HasValue && v.ScreenHeight.HasValue)
                    .GroupBy(v => new { v.ScreenWidth, v.ScreenHeight })
                    .Select(g => new ScreenSizeData
                    {
                        Screen = g.Key.ScreenWidth + " X " + g.Key.ScreenHeight,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Top pages
                DisplayInfo.MPages = context.VisitPages.AsNoTracking()
                    .Where(vp =>
                        EF.Property<int>(vp.visit, "WebsiteID") == Id &&
                        vp.DateCreated >= utcStart &&
                        vp.DateCreated < utcEndExclusive &&
                        (IncludeBots || !vp.visit.IsBot))
                    .GroupBy(vp => new { vp.webpage.Path, vp.webpage.QueryString })
                    .Select(g => new WebpageVisits
                    {
                        Page = string.IsNullOrEmpty(g.Key.QueryString) ? g.Key.Path : g.Key.Path + g.Key.QueryString,
                        AbsoluteUrl = "https://" + website.Name + (string.IsNullOrEmpty(g.Key.QueryString) ? g.Key.Path : g.Key.Path + g.Key.QueryString),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Referrers grouped + expandable raw hosts
                var refRows = visitQuery
                    .GroupBy(v => v.Referer ?? string.Empty)
                    .Select(g => new { Ref = g.Key, Count = g.Count() })
                    .ToList();

                var grouped = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var rawByGroup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in refRows)
                {
                    var (group, rawHost) = ReferrerGrouper.Group(row.Ref);
                    if (!grouped.ContainsKey(group))
                    {
                        grouped[group] = 0;
                        rawByGroup[group] = [];
                    }

                    grouped[group] += row.Count;
                    if (!string.IsNullOrWhiteSpace(rawHost))
                    {
                        rawByGroup[group].Add(rawHost);
                    }
                }

                DisplayInfo.RefererList = grouped
                    .Select(x => new RefererData { Referer = x.Key, Count = x.Value })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                ReferrerDetails = rawByGroup.ToDictionary(k => k.Key, v => v.Value.OrderBy(x => x).ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Report > OnGet");
            }
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
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            var marksToday = localStart.Date <= todayLocal && localEndDate.Date >= todayLocal;

            switch (DisplayInfo.Range)
            {
                case ReportDateRangeCustomType.Day:
                    {
                        var cursor = localStart;
                        while (cursor < endExclusive)
                        {
                            var next = cursor.AddHours(1);
                            var agg = AggregateHours(hourlyUtc, tz, cursor, next);
                            var label = cursor.ToString("h:mm tt");
                            if (marksToday && cursor.Date == todayLocal && next > TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz))
                            {
                                label += " (today partial)";
                            }

                            AddPoint(label, agg);
                            cursor = next;
                        }

                        break;
                    }
                case ReportDateRangeCustomType.Days:
                    {
                        var cursor = localStart.Date;
                        while (cursor < endExclusive)
                        {
                            var next = cursor.AddDays(1);
                            var agg = AggregateHours(hourlyUtc, tz, cursor, next);
                            var label = cursor.ToString("MM/d");
                            if (marksToday && cursor.Date == todayLocal)
                            {
                                label += " (today partial)";
                            }

                            AddPoint(label, agg);
                            cursor = next;
                        }

                        break;
                    }
                case ReportDateRangeCustomType.Months:
                    {
                        var cursor = localStart.Date;
                        while (cursor < endExclusive)
                        {
                            var next = cursor.AddDays(15);
                            if (next > endExclusive)
                            {
                                next = endExclusive;
                            }

                            var agg = AggregateHours(hourlyUtc, tz, cursor, next);
                            var label = cursor.ToString("yyyy-M-d");
                            if (marksToday && cursor <= todayLocal && todayLocal < next)
                            {
                                label += " (today partial)";
                            }

                            AddPoint(label, agg);
                            cursor = next;
                        }

                        break;
                    }
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

            var h = localStartInclusive;
            while (h < localEndExclusive)
            {
                var utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(h, DateTimeKind.Unspecified), tz);
                var key = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
                if (hourlyUtc.TryGetValue(key, out var bucket))
                {
                    all += bucket.All;
                    @new += bucket.New;
                    ret += bucket.Return;
                }

                h = h.AddHours(1);
            }

            return (all, @new, ret);
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
