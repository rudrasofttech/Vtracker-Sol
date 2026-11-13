using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VisitTracker.DataContext;
using VisitTracker.Models;
using VisitTracker.Web.Helpers;

namespace VisitTracker.Web.Controllers
{
    public class RVController(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment, IConfiguration config, ILogger<RVController> _logger) : Controller
    {
        private readonly VisitTrackerDBContext context = _context;
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly VisitManager visitRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);
        private readonly IWebHostEnvironment webHostEnvironment = _webHostEnvironment;
        private readonly IPLocationWorker iPLocationWorker = new(config, _context);
        private readonly ILogger<RVController> logger = _logger;

        private static readonly object MissingHeaderLogSync = new();
        private static DateTime LastMissingHeaderLogUtc = DateTime.MinValue;

        [HttpGet]
        [EnableRateLimiting("rv-policy")]
        public ActionResult Index(string id, Guid cc, string path, string wvri, string a, int w, int h, int st, int sl, int? ms = null, int? dh = null)
        {
            try
            {
                if (!string.Equals(a, "pulse", StringComparison.OrdinalIgnoreCase))
                {
                    return TrackPixel();
                }

                if (!TryResolve(cc, path, out var visit, out _, out var visitPage))
                {
                    return TrackPixel();
                }

                UpdateLastPing(visit.ID, visitPage.ID, w, h, ms, dh);
                return TrackPixel();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RVController > Index");
                return TrackPixel();
            }
        }

        [HttpGet]
        [EnableRateLimiting("rv-policy")]
        public async Task<ActionResult> fr(int wid, Guid cc, string referer, string path, string wvri, string r, string b, int sw, int sh, int st, int sl, string occ, string? vid = null)
        {
            try
            {
                var website = websiteRepository.GetWebsiteByID(wid);
                if (website == null || !UrlNormalizer.TryNormalize(path, out var normalized))
                {
                    return TrackPixel();
                }

                var webpage = webpageRepository.GetWebpage(website.ID, normalized.Path, normalized.QueryString);
                if (webpage == null)
                {
                    webpage = new Webpage
                    {
                        DateCreated = DateTime.UtcNow,
                        Path = normalized.Path,
                        QueryString = normalized.QueryString,
                        Status = RecordStatus.Active,
                        Website = website
                    };
                    webpageRepository.InsertWebpage(webpage);
                    webpageRepository.Save();
                }

                var visit = visitRepository.GetVisitByCC(cc);
                if (visit == null)
                {
                    int? oldVisitID = null;
                    Guid? visitorId = null;

                    if (!string.IsNullOrWhiteSpace(vid) && Guid.TryParse(vid, out var parsedVid))
                    {
                        visitorId = parsedVid;
                    }

                    if (visitorId.HasValue)
                    {
                        var previousByVisitor = context.Visits
                            .Where(v => EF.Property<int>(v, "WebsiteID") == website.ID && v.VisitorId == visitorId.Value)
                            .OrderByDescending(v => v.DateCreated)
                            .Select(v => (int?)v.ID)
                            .FirstOrDefault();

                        if (previousByVisitor.HasValue)
                        {
                            oldVisitID = previousByVisitor.Value;
                        }
                    }
                    else if (!string.IsNullOrEmpty(occ) && Guid.TryParse(occ, out var oldcc))
                    {
                        var oldVisit = visitRepository.GetVisitByCC(oldcc);
                        if (oldVisit != null)
                        {
                            oldVisitID = oldVisit.ID;
                        }
                    }

                    var location = new IP2LocationResult();
                    try
                    {
                        if (Request.HttpContext.Connection.RemoteIpAddress != null)
                        {
                            location = await iPLocationWorker.GetLocationAsync(Request.HttpContext.Connection.RemoteIpAddress.ToString()!);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "RVController > fr > GetLocation");
                    }

                    var userAgent = Truncate(Request.Headers.UserAgent.ToString(), 400);
                    var isBot = BotDetector.IsBot(userAgent, location.Is_Proxy, sw, sh);

                    visit = new Visit
                    {
                        CountryAbbr = string.IsNullOrEmpty(location.Country_Code) ? string.Empty : location.Country_Code,
                        CityName = string.IsNullOrEmpty(location.City_Name) ? string.Empty : location.City_Name,
                        CountryName = string.IsNullOrEmpty(location.Country_Name) ? string.Empty : location.Country_Name,
                        IsProxy = location.Is_Proxy,
                        Latitude = location.Latitude.HasValue ? location.Latitude.Value.ToString() : string.Empty,
                        Longitude = location.Longitude.HasValue ? location.Longitude.Value.ToString() : string.Empty,
                        RegionName = location.Region_Name,
                        ZipCode = location.Zip_Code,
                        DateCreated = DateTime.UtcNow,
                        DateModified = null,
                        IPAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                        LastPingDate = DateTime.UtcNow,
                        Referer = string.IsNullOrEmpty(referer) ? string.Empty : referer,
                        Status = RecordStatus.Active,
                        WebsiteVisitorReferenceID = string.IsNullOrEmpty(wvri) ? string.Empty : wvri,
                        ClientCookie = cc,
                        Website = website,
                        BrowserName = string.IsNullOrEmpty(b) ? string.Empty : b,
                        ScreenHeight = sh,
                        ScreenWidth = sw,
                        LastVisitID = oldVisitID,
                        VisitorId = visitorId,
                        UserAgent = userAgent,
                        IsBot = isBot,
                        UtmSource = normalized.UtmSource,
                        UtmMedium = normalized.UtmMedium,
                        UtmCampaign = normalized.UtmCampaign
                    };

                    visitRepository.InsertVisit(visit);
                    visitRepository.Save();
                }

                var lastVisitPage = visitRepository.GetLastVisitedPageofVisit(visit.ID);
                if (lastVisitPage == null || lastVisitPage.webpage.ID != webpage.ID)
                {
                    var newVisitPage = new VisitPage
                    {
                        LastPingDate = DateTime.UtcNow,
                        visit = visit,
                        webpage = webpage,
                        DateCreated = DateTime.UtcNow,
                        BrowserWidth = null,
                        BrowserHeight = null,
                        ScrollLeft = sl,
                        ScrollTop = st
                    };
                    visitRepository.InsertVisitPage(newVisitPage);
                    visitRepository.Save();
                }

                return TrackPixel();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RVController > fr");
                return TrackPixel();
            }
        }

        [HttpGet]
        [EnableRateLimiting("rv-policy")]
        public ActionResult rc(string id, Guid cc, string path, int x, int y, string tag, string tagid, string? href = null, string? txt = null)
        {
            return TrackActivity(cc, path, visit => new VisitActivity
            {
                Activity = ActivityName.Click,
                ClickTagId = string.IsNullOrEmpty(tagid) ? string.Empty : tagid,
                ClickTagName = string.IsNullOrEmpty(tag) ? string.Empty : tag,
                DateCreated = DateTime.UtcNow,
                MouseClickX = x,
                MouseClickY = y,
                ClickHref = Truncate(href, 500),
                ClickText = Truncate(txt, 100),
                visit = visit.Item1,
                visitpage = visit.Item2
            }, "RVController > rc");
        }

        [HttpGet]
        [EnableRateLimiting("rv-policy")]
        public ActionResult wb(string id, Guid cc, string path, int s)
        {
            return TrackActivity(cc, path, visit => new VisitActivity
            {
                Activity = ActivityName.WindowBlur,
                ClickTagId = string.Empty,
                ClickTagName = string.Empty,
                DateCreated = DateTime.UtcNow,
                MouseClickX = null,
                MouseClickY = null,
                SecondsPassed = s,
                visit = visit.Item1,
                visitpage = visit.Item2
            }, "RVController > wb");
        }

        [HttpGet]
        [EnableRateLimiting("rv-policy")]
        public ActionResult wf(string id, Guid cc, string path, int s)
        {
            return TrackActivity(cc, path, visit => new VisitActivity
            {
                Activity = ActivityName.WindowFocus,
                ClickTagId = string.Empty,
                ClickTagName = string.Empty,
                DateCreated = DateTime.UtcNow,
                MouseClickX = null,
                MouseClickY = null,
                SecondsPassed = s,
                visit = visit.Item1,
                visitpage = visit.Item2
            }, "RVController > wf");
        }

        [HttpGet]
        [EnableRateLimiting("rv-policy")]
        public ActionResult sb(string id, Guid cc, string path)
        {
            return TrackActivity(cc, path, visit => new VisitActivity
            {
                Activity = ActivityName.ScrollBottom,
                ClickTagId = string.Empty,
                ClickTagName = string.Empty,
                DateCreated = DateTime.UtcNow,
                MouseClickX = null,
                MouseClickY = null,
                visit = visit.Item1,
                visitpage = visit.Item2
            }, "RVController > sb");
        }

        [HttpGet]
        public ContentResult getjs(int id)
        {
            try
            {
                SetNoStore();

                var website = websiteRepository.GetWebsiteByID(id);
                if (website == null)
                {
                    return Content(string.Empty, "application/javascript");
                }

                var typedHeaders = Request.GetTypedHeaders();
                var refererUri = typedHeaders.Referer;
                Uri? originUri = null;

                var originRaw = Request.Headers.Origin.ToString();
                if (!string.IsNullOrWhiteSpace(originRaw))
                {
                    Uri.TryCreate(originRaw, UriKind.Absolute, out originUri);
                }

                if (refererUri == null && originUri == null && ShouldLogMissingHeaderInfo())
                {
                    logger.LogInformation("RVController > getjs missing both Referer and Origin headers for websiteId={WebsiteId}", id);
                }

                var load = (refererUri != null && IsHostMatch(website.Name, refererUri.Host))
                    || (originUri != null && IsHostMatch(website.Name, originUri.Host));

                if (!load)
                {
                    return Content(string.Empty, "application/javascript");
                }

                var scriptPath = Path.Combine(webHostEnvironment.WebRootPath, "vt.js");
                var result = System.IO.File.ReadAllText(scriptPath);
                result = result.Replace("var vt_guid = '';", $"var vt_guid = '{Guid.NewGuid()}';");
                return Content(result, "application/javascript");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RVController > getjs");
                SetNoStore();
                return Content(string.Empty, "application/javascript");
            }
        }

        private ActionResult TrackActivity(Guid cc, string path, Func<Tuple<Visit, VisitPage>, VisitActivity> activityFactory, string logContext)
        {
            try
            {
                if (!TryResolve(cc, path, out var visit, out _, out var visitPage))
                {
                    return TrackPixel();
                }

                UpdateLastPing(visit.ID, visitPage.ID);
                var activity = activityFactory(new Tuple<Visit, VisitPage>(visit, visitPage));
                visitRepository.InsertVisitPageActivity(activity);
                visitRepository.Save();

                return TrackPixel();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, logContext);
                return TrackPixel();
            }
        }

        private bool TryResolve(Guid cc, string path, out Visit visit, out Webpage webpage, out VisitPage visitPage)
        {
            visit = null!;
            webpage = null!;
            visitPage = null!;

            if (cc == Guid.Empty || !UrlNormalizer.TryNormalize(path, out var normalized))
            {
                return false;
            }

            visit = visitRepository.GetVisitByCC(cc);
            if (visit == null)
            {
                return false;
            }

            webpage = webpageRepository.GetWebpage(visit.Website.ID, normalized.Path, normalized.QueryString);
            if (webpage == null)
            {
                return false;
            }

            visitPage = visitRepository.GetLastVisitedPageofVisit(visit.ID);
            if (visitPage == null || visitPage.webpage.ID != webpage.ID)
            {
                visitPage = visitRepository.GetVisitPageByVisitAndWebpage(visit.ID, webpage.ID);
            }

            return visitPage != null;
        }

        private void UpdateLastPing(int visitId, int visitPageId, int? browserWidth = null, int? browserHeight = null, int? maxScrollPct = null, int? documentHeight = null)
        {
            var now = DateTime.UtcNow;

            context.Visits
                .Where(v => v.ID == visitId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(v => v.LastPingDate, now));

            var clampedScroll = maxScrollPct.HasValue ? (byte?)Math.Clamp(maxScrollPct.Value, 0, 100) : null;

            context.VisitPages
                .Where(vp => vp.ID == visitPageId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(vp => vp.LastPingDate, now)
                    .SetProperty(vp => vp.BrowserWidth, vp => browserWidth ?? vp.BrowserWidth)
                    .SetProperty(vp => vp.BrowserHeight, vp => browserHeight ?? vp.BrowserHeight)
                    .SetProperty(vp => vp.DocumentHeight, vp => documentHeight ?? vp.DocumentHeight)
                    .SetProperty(
                        vp => vp.MaxScrollPct,
                        vp => clampedScroll.HasValue
                            ? (!vp.MaxScrollPct.HasValue || vp.MaxScrollPct < clampedScroll.Value ? clampedScroll : vp.MaxScrollPct)
                            : vp.MaxScrollPct
                    ));
        }

        private ActionResult TrackPixel()
        {
            SetNoStore();
            return File("~/images/trans.png", "image/png");
        }

        private void SetNoStore()
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
        }

        private static bool IsHostMatch(string websiteHost, string sourceHost)
        {
            return sourceHost.Equals(websiteHost, StringComparison.OrdinalIgnoreCase)
                || sourceHost.EndsWith($".{websiteHost}", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldLogMissingHeaderInfo()
        {
            var now = DateTime.UtcNow;

            lock (MissingHeaderLogSync)
            {
                if (now - LastMissingHeaderLogUtc < TimeSpan.FromMinutes(1))
                {
                    return false;
                }

                LastMissingHeaderLogUtc = now;
                return true;
            }
        }

        private static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var input = value.Trim();
            return input.Length > max ? input[..max] : input;
        }
    }
}


