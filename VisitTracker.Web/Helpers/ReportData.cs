using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Helpers
{
    public sealed class VisitRow
    {
        public int ID { get; init; }
        public DateTime DateCreated { get; init; }
        public DateTime LastPingDate { get; init; }
        public bool IsBot { get; init; }
        public bool? IsProxy { get; init; }
        public int? LastVisitID { get; init; }
        public Guid? VisitorId { get; init; }
        public Guid ClientCookie { get; init; }
        public string? Referer { get; init; }
        public string? UtmSource { get; init; }
        public string? UtmMedium { get; init; }
        public string? UtmCampaign { get; init; }
        public string? UserAgent { get; init; }
        public string? BrowserName { get; init; }
        public int? ScreenWidth { get; init; }
        public int? ScreenHeight { get; init; }
        public string? CountryName { get; init; }
        public string? RegionName { get; init; }
        public string? CityName { get; init; }

        public double Seconds => Math.Clamp((LastPingDate - DateCreated).TotalSeconds, 0, 1800);
        public Guid VisitorKey => VisitorId ?? ClientCookie;
    }

    public sealed class PageViewRow
    {
        public int ID { get; init; }
        public int VisitID { get; init; }
        public string Path { get; init; } = string.Empty;
        public string? Query { get; init; }
        public DateTime DateCreated { get; init; }
        public DateTime LastPingDate { get; init; }
        public byte? MaxScrollPct { get; init; }
        public int? BrowserWidth { get; init; }
        public int? BrowserHeight { get; init; }
        public int? DocumentHeight { get; init; }

        public double Seconds => Math.Clamp((LastPingDate - DateCreated).TotalSeconds, 0, 1800);
        public string Key => VisitAnalytics.PageKey(Path, Query);
    }

    public sealed class ActivityRow
    {
        public int ID { get; init; }
        public int VisitID { get; init; }
        public int? VisitPageID { get; init; }
        public ActivityName Activity { get; init; }
        public DateTime DateCreated { get; init; }
        public int? X { get; init; }
        public int? Y { get; init; }
        public string? Tag { get; init; }
        public string? TagId { get; init; }
        public string? Href { get; init; }
        public string? Text { get; init; }
        public int? Seconds { get; init; }

        // Filled only by the page-behaviour click query.
        public string? PagePath { get; init; }
        public string? PageQuery { get; init; }
        public int? BrowserWidth { get; init; }
        public int? DocumentHeight { get; init; }
        public int? ScreenWidth { get; init; }
    }

    /// <summary>Database reads shared by the Report, Visits and Page behaviour pages.</summary>
    public static class ReportData
    {
        private static readonly Expression<Func<Visit, VisitRow>> VisitProjection = v => new VisitRow
        {
            ID = v.ID,
            DateCreated = v.DateCreated,
            LastPingDate = v.LastPingDate,
            IsBot = v.IsBot,
            IsProxy = v.IsProxy,
            LastVisitID = v.LastVisitID,
            VisitorId = v.VisitorId,
            ClientCookie = v.ClientCookie,
            Referer = v.Referer,
            UtmSource = v.UtmSource,
            UtmMedium = v.UtmMedium,
            UtmCampaign = v.UtmCampaign,
            UserAgent = v.UserAgent,
            BrowserName = v.BrowserName,
            ScreenWidth = v.ScreenWidth,
            ScreenHeight = v.ScreenHeight,
            CountryName = v.CountryName,
            RegionName = v.RegionName,
            CityName = v.CityName
        };

        private static readonly Expression<Func<VisitPage, PageViewRow>> PageViewProjection = vp => new PageViewRow
        {
            ID = vp.ID,
            VisitID = vp.visit.ID,
            Path = vp.webpage.Path,
            Query = vp.webpage.QueryString,
            DateCreated = vp.DateCreated,
            LastPingDate = vp.LastPingDate,
            MaxScrollPct = vp.MaxScrollPct,
            BrowserWidth = vp.BrowserWidth,
            BrowserHeight = vp.BrowserHeight,
            DocumentHeight = vp.DocumentHeight
        };

        private static readonly Expression<Func<VisitActivity, ActivityRow>> ActivityProjection = a => new ActivityRow
        {
            ID = a.ID,
            VisitID = a.visit.ID,
            VisitPageID = (int?)a.visitpage.ID,
            Activity = a.Activity,
            DateCreated = a.DateCreated,
            X = a.MouseClickX,
            Y = a.MouseClickY,
            Tag = a.ClickTagName,
            TagId = a.ClickTagId,
            Href = a.ClickHref,
            Text = a.ClickText,
            Seconds = a.SecondsPassed
        };

        /// <summary>All visits (bots included) that started in [utcFrom, utcTo).</summary>
        public static List<VisitRow> Visits(VisitTrackerDBContext db, int websiteId, DateTime utcFrom, DateTime utcTo) =>
            db.Visits.AsNoTracking()
                .Where(v => EF.Property<int>(v, "WebsiteID") == websiteId && v.DateCreated >= utcFrom && v.DateCreated < utcTo)
                .Select(VisitProjection)
                .ToList();

        public static VisitRow? Visit(VisitTrackerDBContext db, int websiteId, int visitId) =>
            db.Visits.AsNoTracking()
                .Where(v => v.ID == visitId && EF.Property<int>(v, "WebsiteID") == websiteId)
                .Select(VisitProjection)
                .FirstOrDefault();

        /// <summary>Page views of visits that started in [utcFrom, utcTo).</summary>
        public static List<PageViewRow> PageViews(VisitTrackerDBContext db, int websiteId, DateTime utcFrom, DateTime utcTo) =>
            db.VisitPages.AsNoTracking()
                .Where(vp => EF.Property<int>(vp.visit, "WebsiteID") == websiteId
                    && vp.visit.DateCreated >= utcFrom && vp.visit.DateCreated < utcTo)
                .Select(PageViewProjection)
                .ToList();

        public static List<PageViewRow> PageViewsForVisits(VisitTrackerDBContext db, IReadOnlyCollection<int> visitIds) =>
            visitIds.Count == 0
                ? []
                : db.VisitPages.AsNoTracking()
                    .Where(vp => visitIds.Contains(vp.visit.ID))
                    .Select(PageViewProjection)
                    .ToList();

        public static Dictionary<int, int> PageCounts(VisitTrackerDBContext db, int websiteId, DateTime utcFrom, DateTime utcTo) =>
            db.VisitPages.AsNoTracking()
                .Where(vp => EF.Property<int>(vp.visit, "WebsiteID") == websiteId
                    && vp.visit.DateCreated >= utcFrom && vp.visit.DateCreated < utcTo)
                .GroupBy(vp => vp.visit.ID)
                .Select(g => new { VisitID = g.Key, Count = g.Count() })
                .ToDictionary(x => x.VisitID, x => x.Count);

        public static List<ActivityRow> ActivitiesForVisits(VisitTrackerDBContext db, IReadOnlyCollection<int> visitIds) =>
            visitIds.Count == 0
                ? []
                : db.Set<VisitActivity>().AsNoTracking()
                    .Where(a => visitIds.Contains(a.visit.ID))
                    .Select(ActivityProjection)
                    .ToList();

        /// <summary>IDs of visits in the range that clicked at least one link.</summary>
        public static HashSet<int> VisitsWithLinkClicks(VisitTrackerDBContext db, int websiteId, DateTime utcFrom, DateTime utcTo) =>
            db.Set<VisitActivity>().AsNoTracking()
                .Where(a => a.Activity == ActivityName.Click
                    && EF.Property<int>(a.visit, "WebsiteID") == websiteId
                    && a.visit.DateCreated >= utcFrom && a.visit.DateCreated < utcTo
                    && a.ClickHref != null && a.ClickHref != "")
                .Select(a => a.visit.ID)
                .Distinct()
                .ToHashSet();

        /// <summary>Clicks on one page path (all query variants) for visits that started in the range.</summary>
        public static List<ActivityRow> ClicksOnPath(VisitTrackerDBContext db, int websiteId, string path, DateTime utcFrom, DateTime utcTo, bool includeBots) =>
            db.Set<VisitActivity>().AsNoTracking()
                .Where(a => a.Activity == ActivityName.Click
                    && EF.Property<int>(a.visit, "WebsiteID") == websiteId
                    && a.visit.DateCreated >= utcFrom && a.visit.DateCreated < utcTo
                    && (includeBots || !a.visit.IsBot)
                    && a.visitpage.webpage.Path == path)
                .Select(a => new ActivityRow
                {
                    ID = a.ID,
                    VisitID = a.visit.ID,
                    VisitPageID = (int?)a.visitpage.ID,
                    Activity = a.Activity,
                    DateCreated = a.DateCreated,
                    X = a.MouseClickX,
                    Y = a.MouseClickY,
                    Tag = a.ClickTagName,
                    TagId = a.ClickTagId,
                    Href = a.ClickHref,
                    Text = a.ClickText,
                    PagePath = a.visitpage.webpage.Path,
                    PageQuery = a.visitpage.webpage.QueryString,
                    BrowserWidth = a.visitpage.BrowserWidth,
                    DocumentHeight = a.visitpage.DocumentHeight,
                    ScreenWidth = a.visit.ScreenWidth
                })
                .ToList();

        /// <summary>Which visit of this visitor this is (1 = first), when a long-lived visitor ID exists.</summary>
        public static int? VisitNumber(VisitTrackerDBContext db, int websiteId, VisitRow visit) =>
            visit.VisitorId.HasValue
                ? db.Visits.AsNoTracking().Count(v => EF.Property<int>(v, "WebsiteID") == websiteId
                    && v.VisitorId == visit.VisitorId && v.DateCreated <= visit.DateCreated)
                : null;
    }
}
