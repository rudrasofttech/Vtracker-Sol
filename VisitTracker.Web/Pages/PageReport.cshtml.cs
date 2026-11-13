using Microsoft.AspNetCore.Mvc;
using VisitTracker.DataContext;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Pages
{
    public sealed record ScrollReachRow(string Label, int Count, double Pct);

    public sealed record ElementStat(string Kind, string Label, int Clicks, int Visits);

    public sealed record FrustrationStat(string Label, int Count, int Visits, string ExampleRef);

    /// <summary>Click positions for one device class. Points are [x as 0–1 share of viewport width, y in page pixels].</summary>
    public sealed record ClickMapData(string Device, int Width, int Height, int Fold, int Clicks, List<double[]> Points);

    /// <summary>Page behaviour: how visitors used one page (scroll reach, click map, navigation, frustration).</summary>
    public class PageReportModel(VisitTrackerDBContext db, ILogger<PageReportModel> logger) : ReportPageBase(db)
    {
        private const int MobileBreakpoint = 768;
        private const int MaxMapPoints = 4000;

        [FromQuery(Name = "path")] public string? Path { get; set; }

        public string PageKey { get; private set; } = string.Empty;
        public string PageUrl { get; private set; } = string.Empty;
        public bool PageFound { get; private set; }

        public int Views { get; private set; }
        public int Visitors { get; private set; }
        public int Entries { get; private set; }
        public double AvgSeconds { get; private set; }
        public double? AvgScroll { get; private set; }
        public double ExitPct { get; private set; }
        public int ViewsWithScroll { get; private set; }
        public int HiddenBotVisits { get; private set; }
        public int TotalClicks { get; private set; }

        public List<ScrollReachRow> ScrollReach { get; private set; } = [];
        public List<ReportShareRow> CameFrom { get; private set; } = [];
        public List<ReportShareRow> WentTo { get; private set; } = [];
        public List<ClickMapData> ClickMaps { get; private set; } = [];
        public List<ElementStat> TopElements { get; private set; } = [];
        public List<FrustrationStat> RageClicks { get; private set; } = [];
        public List<FrustrationStat> DeadClicks { get; private set; } = [];

        public IActionResult OnGet()
        {
            if (LoadWebsite() == null) return NotFound();
            InitRange();

            var resolved = ResolvePage(Path);
            if (resolved == null) return Page();

            var (pagePath, key) = resolved.Value;
            PageKey = key;
            PageUrl = "https://" + SiteHost + key;

            try
            {
                var all = ReportData.Visits(Db, Id, UtcStart, UtcEnd);
                var visitById = (IncludeBots ? all : all.Where(v => !v.IsBot)).ToDictionary(v => v.ID);

                var pagesByVisit = ReportData.PageViews(Db, Id, UtcStart, UtcEnd)
                    .Where(p => visitById.ContainsKey(p.VisitID))
                    .GroupBy(p => p.VisitID)
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.DateCreated).ThenBy(p => p.ID).ToList());

                var views = pagesByVisit.Values
                    .SelectMany(list => list)
                    .Where(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!IncludeBots)
                {
                    var botIds = all.Where(v => v.IsBot).Select(v => v.ID).ToHashSet();
                    HiddenBotVisits = ReportData.PageViewsForVisits(Db, botIds.ToList())
                        .Where(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.VisitID).Distinct().Count();
                }

                PageFound = views.Count > 0;
                if (!PageFound) return Page();

                BuildSummary(views, pagesByVisit, visitById);
                BuildScrollReach(views);
                BuildNavigation(views, pagesByVisit, visitById);

                var clicks = ReportData.ClicksOnPath(Db, Id, pagePath, UtcStart, UtcEnd, IncludeBots)
                    .Where(c => visitById.ContainsKey(c.VisitID)
                        && VisitAnalytics.PageKey(c.PagePath, c.PageQuery).Equals(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                TotalClicks = clicks.Count;

                BuildClickMaps(views, clicks, visitById);
                BuildElements(clicks);
                BuildFrustration(clicks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PageReport > OnGet");
            }

            return Page();
        }

        private (string Path, string Key)? ResolvePage(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var full = input.Trim();
            if (full.StartsWith('/')) full = "https://" + SiteHost + full;

            if (UrlNormalizer.TryNormalize(full, out var normalized))
            {
                return (normalized.Path, VisitAnalytics.PageKey(normalized.Path, normalized.QueryString));
            }

            if (Uri.TryCreate(full, UriKind.Absolute, out var uri))
            {
                return (uri.AbsolutePath, VisitAnalytics.PageKey(uri.AbsolutePath, uri.Query));
            }

            return null;
        }

        private void BuildSummary(List<PageViewRow> views, Dictionary<int, List<PageViewRow>> pagesByVisit, Dictionary<int, VisitRow> visitById)
        {
            Views = views.Count;
            Visitors = views.Select(v => visitById[v.VisitID].VisitorKey).Distinct().Count();
            Entries = views.Count(v => pagesByVisit[v.VisitID][0].ID == v.ID);
            AvgSeconds = views.Average(v => v.Seconds);
            ExitPct = VisitAnalytics.Pct(views.Count(v => pagesByVisit[v.VisitID][^1].ID == v.ID), views.Count);

            var scrolled = views.Where(v => v.MaxScrollPct.HasValue).ToList();
            ViewsWithScroll = scrolled.Count;
            AvgScroll = scrolled.Count > 0 ? scrolled.Average(v => (double)v.MaxScrollPct!.Value) : null;
        }

        private void BuildScrollReach(List<PageViewRow> views)
        {
            var scrolled = views.Where(v => v.MaxScrollPct.HasValue).Select(v => (int)v.MaxScrollPct!.Value).ToList();
            if (scrolled.Count == 0) return;

            (string Label, int Min)[] steps = [("Top of page", 0), ("25%", 25), ("50%", 50), ("75%", 75), ("Bottom", 98)];
            ScrollReach = steps
                .Select(s =>
                {
                    var count = scrolled.Count(p => p >= s.Min);
                    return new ScrollReachRow(s.Label, count, VisitAnalytics.Pct(count, scrolled.Count));
                })
                .ToList();
        }

        private void BuildNavigation(List<PageViewRow> views, Dictionary<int, List<PageViewRow>> pagesByVisit, Dictionary<int, VisitRow> visitById)
        {
            var from = new List<string>(views.Count);
            var to = new List<string>(views.Count);

            foreach (var view in views)
            {
                var list = pagesByVisit[view.VisitID];
                var i = list.FindIndex(p => p.ID == view.ID);

                if (i > 0)
                {
                    from.Add(SameOrPage(list[i - 1].Key));
                }
                else
                {
                    var visit = visitById[view.VisitID];
                    var (group, _) = ReferrerGrouper.Group(visit.Referer, SiteHost);
                    from.Add("Started here · " + VisitAnalytics.Channel(group, visit.UtmSource, visit.UtmMedium, visit.UserAgent, view.Query));
                }

                to.Add(i >= 0 && i < list.Count - 1 ? SameOrPage(list[i + 1].Key) : "Left the site");
            }

            CameFrom = Share(from, 12);
            WentTo = Share(to, 12);
        }

        private string SameOrPage(string key) =>
            key.Equals(PageKey, StringComparison.OrdinalIgnoreCase) ? "This page again (reload)" : key;

        private void BuildClickMaps(List<PageViewRow> views, List<ActivityRow> clicks, Dictionary<int, VisitRow> visitById)
        {
            string? DeviceOf(int? width) => width is > 0 ? (width < MobileBreakpoint ? "Mobile" : "Desktop") : null;

            foreach (var device in new[] { "Mobile", "Desktop" })
            {
                var deviceViews = views
                    .Where(v => DeviceOf(v.BrowserWidth ?? visitById[v.VisitID].ScreenWidth) == device)
                    .ToList();

                var deviceClicks = clicks
                    .Where(c => c.X.HasValue && c.Y.HasValue && DeviceOf(c.BrowserWidth ?? c.ScreenWidth) == device)
                    .ToList();

                if (deviceClicks.Count == 0) continue;

                var width = Median(deviceClicks.Select(c => c.BrowserWidth ?? c.ScreenWidth ?? 0).Where(w => w > 0))
                    ?? (device == "Mobile" ? 390 : 1366);
                var height = Median(deviceViews.Where(v => v.DocumentHeight > 0).Select(v => v.DocumentHeight!.Value))
                    ?? Math.Max(deviceClicks.Max(c => c.Y!.Value) + 200, 1000);
                var fold = Median(deviceViews.Where(v => v.BrowserHeight > 0).Select(v => v.BrowserHeight!.Value))
                    ?? (device == "Mobile" ? 700 : 760);

                var points = deviceClicks
                    .OrderByDescending(c => c.DateCreated)
                    .Take(MaxMapPoints)
                    .Select(c =>
                    {
                        var w = c.BrowserWidth ?? c.ScreenWidth ?? width;
                        var x = Math.Clamp((double)c.X!.Value / Math.Max(1, w), 0, 1);
                        return new[] { Math.Round(x, 4), (double)c.Y!.Value };
                    })
                    .ToList();

                ClickMaps.Add(new ClickMapData(device, width, Math.Max(height, fold), fold, deviceClicks.Count, points));
            }
        }

        private void BuildElements(List<ActivityRow> clicks)
        {
            TopElements = clicks
                .Select(c =>
                {
                    if (!string.IsNullOrWhiteSpace(c.Href))
                    {
                        var (kind, target) = VisitAnalytics.ClassifyClick(c.Href!, SiteHost);
                        var text = string.IsNullOrWhiteSpace(c.Text) ? "" : $"“{c.Text!.Trim()}” → ";
                        return (Kind: kind, Label: text + target, c.VisitID);
                    }
                    var dead = VisitAnalytics.IsDeadClick(c.Tag, c.Href);
                    return (Kind: dead ? "Not a link" : "Control", Label: VisitAnalytics.ElementLabel(c.Tag, c.TagId, c.Text), c.VisitID);
                })
                .GroupBy(x => (x.Kind, x.Label))
                .Select(g => new ElementStat(g.Key.Kind, g.Key.Label, g.Count(), g.Select(x => x.VisitID).Distinct().Count()))
                .OrderByDescending(e => e.Clicks)
                .Take(25)
                .ToList();
        }

        private void BuildFrustration(List<ActivityRow> clicks)
        {
            RageClicks = VisitAnalytics.FindRageClusters(clicks)
                .GroupBy(cluster => VisitAnalytics.ElementLabel(cluster[0].Tag, cluster[0].TagId, cluster[0].Text))
                .Select(g => new FrustrationStat(
                    g.Key,
                    g.Count(),
                    g.Select(c => c[0].VisitID).Distinct().Count(),
                    VisitAnalytics.VisitRef(g.OrderByDescending(c => c[0].DateCreated).First()[0].VisitID)))
                .OrderByDescending(f => f.Count)
                .Take(15)
                .ToList();

            DeadClicks = clicks
                .Where(c => VisitAnalytics.IsDeadClick(c.Tag, c.Href))
                .GroupBy(c => VisitAnalytics.ElementLabel(c.Tag, c.TagId, c.Text))
                .Select(g => new FrustrationStat(
                    g.Key,
                    g.Count(),
                    g.Select(c => c.VisitID).Distinct().Count(),
                    VisitAnalytics.VisitRef(g.OrderByDescending(c => c.DateCreated).First().VisitID)))
                .OrderByDescending(f => f.Count)
                .Take(15)
                .ToList();
        }

        private static List<ReportShareRow> Share(List<string> items, int take) =>
            items.GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ReportShareRow(g.Key, g.Count(), VisitAnalytics.Pct(g.Count(), items.Count)))
                .OrderByDescending(r => r.Count)
                .Take(take)
                .ToList();

        private static int? Median(IEnumerable<int> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            return sorted.Count == 0 ? null : sorted[sorted.Count / 2];
        }
    }
}
