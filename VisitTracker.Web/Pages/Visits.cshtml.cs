using Microsoft.AspNetCore.Mvc;
using VisitTracker.DataContext;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Pages
{
    public sealed record FilterOption(string Value, string Label, int Count);

    public class VisitsModel(VisitTrackerDBContext db, ILogger<VisitsModel> logger) : ReportPageBase(db)
    {
        public const int PageSize = 25;

        [FromQuery(Name = "ch")] public string? ChannelFilter { get; set; }
        [FromQuery(Name = "lp")] public string? LandingFilter { get; set; }
        [FromQuery(Name = "dev")] public string? DeviceFilter { get; set; }
        [FromQuery(Name = "act")] public int? ActionsOnly { get; set; }
        [FromQuery(Name = "eng")] public int? EngagedOnly { get; set; }
        [FromQuery(Name = "pg")] public int PageNumber { get; set; } = 1;
        [FromQuery(Name = "open")] public string? Open { get; set; }

        public List<VisitSummary> Visits { get; private set; } = [];
        public int TotalInRange { get; private set; }
        public int TotalMatching { get; private set; }
        public int TotalPages { get; private set; }
        public int HiddenBotVisits { get; private set; }
        public string? OpenRef { get; private set; }

        public List<FilterOption> ChannelOptions { get; private set; } = [];
        public List<FilterOption> LandingOptions { get; private set; } = [];
        public List<FilterOption> DeviceOptions { get; private set; } = [];

        public bool HasFilters =>
            !string.IsNullOrEmpty(ChannelFilter) || !string.IsNullOrEmpty(LandingFilter) || !string.IsNullOrEmpty(DeviceFilter)
            || ActionsOnly == 1 || EngagedOnly == 1;

        private sealed record Classified(VisitRow Visit, string Channel, string Landing, string Device, int Pages);

        public IActionResult OnGet()
        {
            if (LoadWebsite() == null) return NotFound();
            InitRange();

            if (VisitAnalytics.VisitIdFromRef(Open).HasValue) OpenRef = Open!.Trim().ToLowerInvariant();

            try
            {
                var all = ReportData.Visits(Db, Id, UtcStart, UtcEnd);
                HiddenBotVisits = IncludeBots ? 0 : all.Count(v => v.IsBot);
                var visits = IncludeBots ? all : all.Where(v => !v.IsBot).ToList();
                TotalInRange = visits.Count;

                var ids = visits.Select(v => v.ID).ToHashSet();
                var pagesByVisit = ReportData.PageViews(Db, Id, UtcStart, UtcEnd)
                    .Where(p => ids.Contains(p.VisitID))
                    .GroupBy(p => p.VisitID)
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.DateCreated).ThenBy(p => p.ID).ToList());

                var classified = visits.Select(v =>
                {
                    var pages = pagesByVisit.GetValueOrDefault(v.ID) ?? [];
                    var (group, _) = ReferrerGrouper.Group(v.Referer, SiteHost);
                    return new Classified(
                        v,
                        VisitAnalytics.Channel(group, v.UtmSource, v.UtmMedium, v.UserAgent, pages.FirstOrDefault()?.Query),
                        pages.FirstOrDefault()?.Key ?? "–",
                        VisitAnalytics.DeviceType(v.ScreenWidth, v.ScreenHeight, v.UserAgent),
                        pages.Count);
                }).ToList();

                ChannelOptions = Options(classified, c => c.Channel, 20);
                LandingOptions = Options(classified, c => c.Landing, 30);
                DeviceOptions = Options(classified, c => c.Device, 5);

                IEnumerable<Classified> filtered = classified;
                if (!string.IsNullOrEmpty(ChannelFilter)) filtered = filtered.Where(c => c.Channel.Equals(ChannelFilter, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(LandingFilter)) filtered = filtered.Where(c => c.Landing.Equals(LandingFilter, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(DeviceFilter)) filtered = filtered.Where(c => c.Device.Equals(DeviceFilter, StringComparison.OrdinalIgnoreCase));
                if (EngagedOnly == 1) filtered = filtered.Where(c => c.Pages > 1 || c.Visit.Seconds >= 30);
                if (ActionsOnly == 1)
                {
                    var withClicks = ReportData.VisitsWithLinkClicks(Db, Id, UtcStart, UtcEnd);
                    filtered = filtered.Where(c => withClicks.Contains(c.Visit.ID));
                }

                var matching = filtered.OrderByDescending(c => c.Visit.DateCreated).ToList();
                TotalMatching = matching.Count;
                TotalPages = Math.Max(1, (int)Math.Ceiling(TotalMatching / (double)PageSize));
                PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

                var slice = matching.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
                var activities = ReportData.ActivitiesForVisits(Db, slice.Select(c => c.Visit.ID).ToList())
                    .GroupBy(a => a.VisitID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                Visits = slice
                    .Select(c => VisitJourneys.Summarize(
                        c.Visit,
                        pagesByVisit.GetValueOrDefault(c.Visit.ID) ?? [],
                        activities.GetValueOrDefault(c.Visit.ID) ?? [],
                        SiteHost,
                        ToLocal))
                    .ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Visits > OnGet");
            }

            return Page();
        }

        /// <summary>Returns the journey timeline of one visit as HTML, loaded when a row is opened.</summary>
        public IActionResult OnGetJourney(string v)
        {
            if (LoadWebsite() == null) return NotFound();
            InitRange();

            var visitId = VisitAnalytics.VisitIdFromRef(v);
            if (!visitId.HasValue) return NotFound();

            var visit = ReportData.Visit(Db, Id, visitId.Value);
            if (visit == null) return NotFound();

            var pages = ReportData.PageViewsForVisits(Db, [visit.ID])
                .OrderBy(p => p.DateCreated).ThenBy(p => p.ID)
                .ToList();
            var activities = ReportData.ActivitiesForVisits(Db, [visit.ID]);
            var number = ReportData.VisitNumber(Db, Id, visit);

            var journey = VisitJourneys.Build(visit, pages, activities, SiteHost, ToLocal, number,
                url => RangeUrl("/PageReport", ("path", url)));

            return Partial("_VisitJourney", journey);
        }

        /// <summary>Link to this page with the current filters, at the given list page.</summary>
        public string FilterUrl(int page) => RangeUrl("/Visits",
            ("ch", ChannelFilter), ("lp", LandingFilter), ("dev", DeviceFilter),
            ("act", ActionsOnly == 1 ? "1" : null), ("eng", EngagedOnly == 1 ? "1" : null),
            ("pg", page > 1 ? page.ToString() : null));

        private static List<FilterOption> Options(IEnumerable<Classified> items, Func<Classified, string> key, int take) =>
            items.GroupBy(key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new FilterOption(g.Key, g.Key, g.Count()))
                .OrderByDescending(o => o.Count)
                .Take(take)
                .ToList();
    }
}
