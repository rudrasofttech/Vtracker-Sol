using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitTracker.DataContext;
using VisitTracker.Models;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Pages
{
    public sealed record ReportKpis(int Visits, int Visitors, int PageViews, double PagesPerVisit, double AvgSeconds, double BouncePct, double ReturningPct)
    {
        public static readonly ReportKpis Empty = new(0, 0, 0, 0, 0, 0, 0);
    }

    public sealed record ReportChartPoint(string Label, int Visits, int New, int Returning, int Previous);

    public sealed record ReportShareRow(string Name, int Count, double Pct);

    public sealed record ReportReferrerRow(string Group, int Count, double Pct, IReadOnlyList<string> Hosts);

    public sealed record ReportPageStat(string Page, string Url, int Views, int Visitors, int Entries, double AvgSeconds, double? AvgScroll, double ExitPct);

    public sealed record ReportClickStat(string Kind, string Target, string Text, int Clicks);

    public class ReportModel(VisitTrackerDBContext db, ILogger<ReportModel> logger) : ReportPageBase(db)
    {
        public static readonly string[] WeekdayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

        public string PreviousDescription { get; private set; } = string.Empty;

        public ReportKpis Current { get; private set; } = ReportKpis.Empty;
        public ReportKpis Previous { get; private set; } = ReportKpis.Empty;
        public List<ReportChartPoint> Series { get; private set; } = [];

        public List<ReportShareRow> Channels { get; private set; } = [];
        public List<ReportReferrerRow> Referrers { get; private set; } = [];
        public List<ReportShareRow> Campaigns { get; private set; } = [];
        public List<ReportPageStat> Pages { get; private set; } = [];
        public List<ReportClickStat> Clicks { get; private set; } = [];
        public List<VisitSummary> RecentVisits { get; private set; } = [];
        public List<ReportShareRow> Devices { get; private set; } = [];
        public List<ReportShareRow> OperatingSystems { get; private set; } = [];
        public List<ReportShareRow> Browsers { get; private set; } = [];
        public List<ReportShareRow> Countries { get; private set; } = [];
        public List<ReportShareRow> Regions { get; private set; } = [];
        public List<ReportShareRow> Cities { get; private set; } = [];

        /// <summary>[weekday Mon..Sun][hour 0..23] visit counts in the website's time zone.</summary>
        public int[][] Heatmap { get; private set; } = Enumerable.Range(0, 7).Select(_ => new int[24]).ToArray();
        public int HeatmapMax { get; private set; }

        public int HiddenBotVisits { get; private set; }
        public int ProxyVisits { get; private set; }
        public double UserAgentCoveragePct { get; private set; }
        public double ScrollCoveragePct { get; private set; }

        public bool HasData => Current.Visits > 0;

        public IActionResult OnGet()
        {
            if (LoadWebsite() == null)
            {
                logger.LogWarning("Report > website {Id} not found", Id);
                return NotFound();
            }

            InitRange();

            // Previous period of the same length, cut off at the same point in time,
            // so "Today" at 3 PM is compared with yesterday until 3 PM.
            var span = LocalEnd - LocalStart;
            var effectiveEnd = LocalEnd < NowLocal ? LocalEnd : NowLocal;
            var compareSpan = effectiveEnd > LocalStart ? effectiveEnd - LocalStart : span;
            var prevStart = LocalStart - span;
            var prevEnd = prevStart + compareSpan;
            PreviousDescription = Describe(prevStart, prevEnd, Bucket);

            try
            {
                var allVisits = ReportData.Visits(Db, Id, UtcStart, UtcEnd);
                HiddenBotVisits = IncludeBots ? 0 : allVisits.Count(v => v.IsBot);
                var visits = IncludeBots ? allVisits : allVisits.Where(v => !v.IsBot).ToList();

                var visitById = visits.ToDictionary(v => v.ID);
                var pageViews = ReportData.PageViews(Db, Id, UtcStart, UtcEnd)
                    .Where(p => visitById.ContainsKey(p.VisitID))
                    .ToList();
                var pagesByVisit = pageViews
                    .GroupBy(p => p.VisitID)
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.DateCreated).ThenBy(p => p.ID).ToList());

                Current = ComputeKpis(visits, id => pagesByVisit.TryGetValue(id, out var list) ? list.Count : 0);

                var prevVisits = ReportData.Visits(Db, Id, ToUtc(prevStart), ToUtc(prevEnd))
                    .Where(v => IncludeBots || !v.IsBot)
                    .ToList();
                var prevPageCounts = ReportData.PageCounts(Db, Id, ToUtc(prevStart), ToUtc(prevEnd));
                Previous = ComputeKpis(prevVisits, id => prevPageCounts.TryGetValue(id, out var c) ? c : 0);

                BuildSeries(visits, prevVisits, span);

                if (visits.Count > 0)
                {
                    BuildSources(visits, pagesByVisit);
                    BuildPages(pageViews, pagesByVisit, visitById);
                    BuildAudience(visits);
                    BuildHeatmap(visits);
                    BuildRecentVisits(visits, pagesByVisit);

                    ProxyVisits = visits.Count(v => v.IsProxy == true);
                    UserAgentCoveragePct = VisitAnalytics.Pct(visits.Count(v => !string.IsNullOrWhiteSpace(v.UserAgent)), visits.Count);
                    ScrollCoveragePct = VisitAnalytics.Pct(pageViews.Count(p => p.MaxScrollPct.HasValue), pageViews.Count);
                }

                BuildClicks();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Report > OnGet");
            }

            return Page();
        }

        // ---------- Summary numbers ----------

        private static ReportKpis ComputeKpis(List<VisitRow> visits, Func<int, int> pageCount)
        {
            if (visits.Count == 0) return ReportKpis.Empty;

            int pageViews = 0, bounces = 0;
            double seconds = 0;

            foreach (var v in visits)
            {
                var pages = pageCount(v.ID);
                pageViews += pages;
                seconds += v.Seconds;
                if (pages <= 1 && v.Seconds < 10) bounces++;
            }

            return new ReportKpis(
                Visits: visits.Count,
                Visitors: visits.Select(v => v.VisitorKey).Distinct().Count(),
                PageViews: pageViews,
                PagesPerVisit: (double)pageViews / visits.Count,
                AvgSeconds: seconds / visits.Count,
                BouncePct: VisitAnalytics.Pct(bounces, visits.Count),
                ReturningPct: VisitAnalytics.Pct(visits.Count(v => v.LastVisitID.HasValue), visits.Count));
        }

        // ---------- Trend chart ----------

        private void BuildSeries(List<VisitRow> visits, List<VisitRow> prevVisits, TimeSpan span)
        {
            var currentHour = new DateTime(NowLocal.Year, NowLocal.Month, NowLocal.Day, NowLocal.Hour, 0, 0);
            var starts = new List<DateTime>();

            switch (Bucket)
            {
                case ReportDateRangeCustomType.Day:
                    var plotEnd = LocalEnd > currentHour.AddHours(1) ? currentHour.AddHours(1) : LocalEnd;
                    for (var t = LocalStart; t < plotEnd; t = t.AddHours(1)) starts.Add(t);
                    break;
                case ReportDateRangeCustomType.Days:
                    for (var t = LocalStart.Date; t < LocalEnd; t = t.AddDays(1)) starts.Add(t);
                    break;
                default:
                    for (var t = LocalStart.Date; t < LocalEnd; t = t.AddDays(15)) starts.Add(t);
                    break;
            }

            var n = starts.Count;
            var all = new int[n];
            var fresh = new int[n];
            var returning = new int[n];
            var previous = new int[n];

            int IndexOf(DateTime local) => Bucket switch
            {
                ReportDateRangeCustomType.Day => (int)Math.Floor((local - LocalStart).TotalHours),
                ReportDateRangeCustomType.Days => (local.Date - LocalStart.Date).Days,
                _ => (local.Date - LocalStart.Date).Days / 15
            };

            foreach (var v in visits)
            {
                var i = IndexOf(ToLocal(v.DateCreated));
                if (i < 0 || i >= n) continue;
                all[i]++;
                if (v.LastVisitID.HasValue) returning[i]++; else fresh[i]++;
            }

            foreach (var v in prevVisits)
            {
                var i = IndexOf(ToLocal(v.DateCreated) + span);
                if (i >= 0 && i < n) previous[i]++;
            }

            var multiDay = LocalStart.Date != LocalEnd.AddTicks(-1).Date;
            var today = NowLocal.Date;

            Series = starts.Select((t, i) =>
            {
                string label = Bucket switch
                {
                    ReportDateRangeCustomType.Day => t.ToString(multiDay ? "ddd h tt" : "h tt") + (t == currentHour ? " (now)" : ""),
                    ReportDateRangeCustomType.Days => t.ToString("d MMM") + (t == today ? " (today)" : ""),
                    _ => t.ToString("d MMM yyyy")
                };
                return new ReportChartPoint(label, all[i], fresh[i], returning[i], previous[i]);
            }).ToList();
        }

        // ---------- Traffic sources ----------

        private void BuildSources(List<VisitRow> visits, Dictionary<int, List<PageViewRow>> pagesByVisit)
        {
            var total = visits.Count;
            var channels = new List<string>(total);
            var referrerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var referrerHosts = new Dictionary<string, SortedSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var v in visits)
            {
                var landingQuery = pagesByVisit.TryGetValue(v.ID, out var pages) && pages.Count > 0 ? pages[0].Query : null;
                var (group, host) = ReferrerGrouper.Group(v.Referer, SiteHost);
                channels.Add(VisitAnalytics.Channel(group, v.UtmSource, v.UtmMedium, v.UserAgent, landingQuery));

                referrerCounts[group] = referrerCounts.GetValueOrDefault(group) + 1;
                if (!referrerHosts.TryGetValue(group, out var hosts))
                {
                    hosts = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                    referrerHosts[group] = hosts;
                }
                if (!string.IsNullOrWhiteSpace(host)) hosts.Add(host);
            }

            Channels = Share(channels, c => c, total, 20);

            Referrers = referrerCounts
                .Select(x => new ReportReferrerRow(x.Key, x.Value, VisitAnalytics.Pct(x.Value, total), referrerHosts[x.Key].ToList()))
                .OrderByDescending(x => x.Count)
                .ToList();

            Campaigns = Share(
                visits.Where(v => !string.IsNullOrWhiteSpace(v.UtmCampaign) || !string.IsNullOrWhiteSpace(v.UtmSource)),
                v => $"{(string.IsNullOrWhiteSpace(v.UtmCampaign) ? "(no campaign name)" : v.UtmCampaign)} ({v.UtmSource ?? "?"} / {v.UtmMedium ?? "?"})",
                total, 15);
        }

        // ---------- Pages ----------

        private void BuildPages(List<PageViewRow> pageViews, Dictionary<int, List<PageViewRow>> pagesByVisit, Dictionary<int, VisitRow> visitById)
        {
            var entries = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var exits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var list in pagesByVisit.Values)
            {
                entries[list[0].Key] = entries.GetValueOrDefault(list[0].Key) + 1;
                exits[list[^1].Key] = exits.GetValueOrDefault(list[^1].Key) + 1;
            }

            Pages = pageViews
                .GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var views = g.Count();
                    var scrolls = g.Where(p => p.MaxScrollPct.HasValue).Select(p => (double)p.MaxScrollPct!.Value).ToList();
                    return new ReportPageStat(
                        Page: g.Key,
                        Url: "https://" + SiteHost + g.Key,
                        Views: views,
                        Visitors: g.Select(p => visitById[p.VisitID].VisitorKey).Distinct().Count(),
                        Entries: entries.GetValueOrDefault(g.Key),
                        AvgSeconds: g.Average(p => p.Seconds),
                        AvgScroll: scrolls.Count > 0 ? scrolls.Average() : null,
                        ExitPct: VisitAnalytics.Pct(exits.GetValueOrDefault(g.Key), views));
                })
                .OrderByDescending(p => p.Views)
                .ToList();
        }

        // ---------- Recent visits ----------

        private void BuildRecentVisits(List<VisitRow> visits, Dictionary<int, List<PageViewRow>> pagesByVisit)
        {
            var recent = visits.OrderByDescending(v => v.DateCreated).Take(10).ToList();
            var activities = ReportData.ActivitiesForVisits(Db, recent.Select(v => v.ID).ToList())
                .GroupBy(a => a.VisitID)
                .ToDictionary(g => g.Key, g => g.ToList());

            RecentVisits = recent
                .Select(v => VisitJourneys.Summarize(
                    v,
                    pagesByVisit.GetValueOrDefault(v.ID) ?? [],
                    activities.GetValueOrDefault(v.ID) ?? [],
                    SiteHost,
                    ToLocal))
                .ToList();
        }

        // ---------- Clicks ----------

        private void BuildClicks()
        {
            var raw = Db.Set<VisitActivity>().AsNoTracking()
                .Where(a => a.Activity == ActivityName.Click
                    && EF.Property<int>(a.visit, "WebsiteID") == Id
                    && a.visit.DateCreated >= UtcStart && a.visit.DateCreated < UtcEnd
                    && (IncludeBots || !a.visit.IsBot)
                    && a.ClickHref != null && a.ClickHref != "")
                .GroupBy(a => new { a.ClickHref, a.ClickText })
                .Select(g => new { Href = g.Key.ClickHref, Text = g.Key.ClickText, Count = g.Count() })
                .ToList();

            Clicks = raw
                .Select(r => (Target: VisitAnalytics.ClassifyClick(r.Href!, SiteHost), r.Text, r.Count))
                .Where(r => r.Target.Kind != "In-page")
                .GroupBy(r => r.Target)
                .Select(g => new ReportClickStat(
                    g.Key.Kind,
                    g.Key.Target,
                    g.Where(x => !string.IsNullOrWhiteSpace(x.Text))
                     .OrderByDescending(x => x.Count)
                     .Select(x => x.Text!.Trim())
                     .FirstOrDefault() ?? string.Empty,
                    g.Sum(x => x.Count)))
                .OrderByDescending(c => c.Clicks)
                .Take(50)
                .ToList();
        }

        // ---------- Audience ----------

        private void BuildAudience(List<VisitRow> visits)
        {
            var total = visits.Count;
            Devices = Share(visits, v => VisitAnalytics.DeviceType(v.ScreenWidth, v.ScreenHeight, v.UserAgent), total, 5);
            OperatingSystems = Share(visits, v => VisitAnalytics.OperatingSystem(v.UserAgent), total, 8);
            Browsers = Share(visits, v => VisitAnalytics.Browser(v.UserAgent, v.BrowserName), total, 10);
            Countries = Share(visits, v => VisitAnalytics.Blank(v.CountryName), total, 10);
            Regions = Share(visits, v => VisitAnalytics.Blank(v.RegionName), total, 15);
            Cities = Share(visits, v => VisitAnalytics.Blank(v.CityName), total, 15);
        }

        private void BuildHeatmap(List<VisitRow> visits)
        {
            foreach (var v in visits)
            {
                var local = ToLocal(v.DateCreated);
                var day = ((int)local.DayOfWeek + 6) % 7; // Monday = 0
                Heatmap[day][local.Hour]++;
            }
            HeatmapMax = Heatmap.SelectMany(r => r).DefaultIfEmpty(0).Max();
        }

        private static List<ReportShareRow> Share<T>(IEnumerable<T> items, Func<T, string> key, int total, int take) =>
            items.GroupBy(key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ReportShareRow(g.Key, g.Count(), VisitAnalytics.Pct(g.Count(), total)))
                .OrderByDescending(r => r.Count)
                .Take(take)
                .ToList();
    }
}
