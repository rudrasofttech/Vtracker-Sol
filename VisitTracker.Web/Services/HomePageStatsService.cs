using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Services
{
    public class HomePageStatsService(VisitTrackerDBContext context, IMemoryCache memoryCache, ILogger<HomePageStatsService> logger)
    {
        private const string CacheKey = "home-page-stats-v1";
        private readonly VisitTrackerDBContext _context = context;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly ILogger<HomePageStatsService> _logger = logger;

        public HomePageStatsSnapshot GetSnapshot()
        {
            return _memoryCache.GetOrCreate(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                try
                {
                    return BuildSnapshot();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HomePageStatsService > BuildSnapshot failed");
                    return new HomePageStatsSnapshot();
                }
            })!;
        }

        private HomePageStatsSnapshot BuildSnapshot()
        {
            var activeWebsites = _context.Websites
                .AsNoTracking()
                .Where(w => w.Status == RecordStatus.Active)
                .Select(w => new { w.ID, w.Name, w.ClientID, w.DateCreated, w.DateModified, w.Status, w.OwnerId })
                .ToList();

            Dictionary<int, int> visitCounts = [];
            Dictionary<int, int> activityCounts = [];
            Dictionary<int, int> webpageCounts = [];

            var activeWebsiteIds = activeWebsites.Select(w => w.ID).ToHashSet();

            try
            {
                visitCounts = _context.Visits
                    .AsNoTracking()
                    .GroupBy(v => EF.Property<int>(v, "WebsiteID"))
                    .Select(g => new { WebsiteId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.WebsiteId, x => x.Count);

                visitCounts = visitCounts
                    .Where(kvp => activeWebsiteIds.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HomePageStatsService > Visit aggregate failed");
            }

            try
            {
                activityCounts = _context.VisitActivities
                    .AsNoTracking()
                    .GroupBy(a => EF.Property<int>(a.visit, "WebsiteID"))
                    .Select(g => new { WebsiteId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.WebsiteId, x => x.Count);

                activityCounts = activityCounts
                    .Where(kvp => activeWebsiteIds.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HomePageStatsService > Activity aggregate failed");
            }

            try
            {
                webpageCounts = _context.Webpages
                    .AsNoTracking()
                    .Where(p => p.Status == RecordStatus.Active)
                    .GroupBy(p => EF.Property<int>(p, "WebsiteID"))
                    .Select(g => new { WebsiteId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.WebsiteId, x => x.Count);

                webpageCounts = webpageCounts
                    .Where(kvp => activeWebsiteIds.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HomePageStatsService > Webpage aggregate failed");
            }

            var stats = activeWebsites.Select(w => new WebsiteStats
            {
                Site = new Website
                {
                    ID = w.ID,
                    Name = w.Name,
                    ClientID = w.ClientID,
                    DateCreated = w.DateCreated,
                    DateModified = w.DateModified,
                    Status = w.Status,
                    OwnerId = w.OwnerId
                },
                VisitCount = visitCounts.TryGetValue(w.ID, out var vc) ? vc : 0,
                ActivityCount = activityCounts.TryGetValue(w.ID, out var ac) ? ac : 0
            }).ToList();

            return new HomePageStatsSnapshot
            {
                Stats = stats,
                WebsiteCount = activeWebsites.Count,
                WebpageCount = webpageCounts.Values.Sum(),
                VisitCount = visitCounts.Values.Sum(),
                ActivityCount = activityCounts.Values.Sum()
            };
        }
    }

    public class HomePageStatsSnapshot
    {
        public List<WebsiteStats> Stats { get; set; } = [];
        public int VisitCount { get; set; }
        public int ActivityCount { get; set; }
        public int WebsiteCount { get; set; }
        public int WebpageCount { get; set; }
    }
}