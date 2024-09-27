using Azure.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitTracker.DataContext;
using VisitTracker.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VisitTracker.Web.Controllers
{
    public class RVController(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment, IConfiguration config, ILogger<RVController> _logger) : Controller
    {
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly VisitManager visitRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);
        private readonly IWebHostEnvironment webHostEnvironment = _webHostEnvironment;
        private readonly IPLocationWorker iPLocationWorker = new(config, _context);
        private readonly ILogger<RVController> logger = _logger;
        //private readonly VisitTrackerDBContext context = _context;

        /// <summary>
        /// Action will record a pulse
        /// </summary>
        /// <param name="id">Randomize Request</param>
        /// <param name="cc">Client Cookie</param>
        /// <param name="path">Page Path</param>
        /// <param name="wvri">Website Visitor Reference ID</param>
        /// <param name="a">Action</param>
        /// <param name="w">Browser Width</param>
        /// <param name="h">Browser Height</param>
        /// <param name="st">Scroll Top</param>
        /// <param name="sl">scroll Left</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string id, Guid cc, string path, string wvri, string a, int w, int h, int st, int sl)
        {
            if (a == "pulse")
            {
                Visit? v = visitRepository.GetVisitByCC(cc);
                VisitPage? vp = null;
                Webpage? wp = null;
                if (v != null)
                {
                    v.LastPingDate = DateTime.UtcNow;
                    visitRepository.UpdateVisit(v);
                    visitRepository.Save();

                    Uri u = new Uri(path);
                    wp = webpageRepository.GetWebpage(v.Website.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                    if (wp != null)
                    {
                        vp = visitRepository.GetLastVisitedPageofVisit(v.ID);

                        if (vp != null && vp.webpage.ID == wp.ID)
                        {
                            vp.LastPingDate = DateTime.UtcNow;
                            vp.BrowserHeight = h;
                            vp.BrowserWidth = w;
                            visitRepository.UpdateVisitPage(vp);
                            visitRepository.Save();
                        }
                        else
                        {
                            vp = new VisitPage()
                            {
                                LastPingDate = DateTime.UtcNow,
                                visit = v,
                                webpage = wp,
                                DateCreated = DateTime.UtcNow,
                                BrowserHeight = h,
                                BrowserWidth = w,
                                ScrollTop = st,
                                ScrollLeft = sl
                            };
                            visitRepository.InsertVisitPage(vp);
                            visitRepository.Save();
                        }
                    }
                }
            }
            return base.File("~/images/trans.png", "image/png");
        }

        /// <summary>
        /// Action records first reqeust to a clients page
        /// </summary>
        /// <param name="wid">Website ID</param>
        /// <param name="cc">Client Cookie GUID</param>
        /// <param name="referer">Referer</param>
        /// <param name="path"></param>
        /// <param name="wvri">Website-Visitor-Refering-ID</param>
        /// <param name="r">Randomize Request</param>
        /// <param name="b">Browser</param>
        /// <param name="sw">Screen Width</param>
        /// <param name="sh">Screen Height</param>
        /// <param name="st">Scroll Top</param>
        /// <param name="sl">Scroll Left</param>
        /// <param name="occ">Old Client Cookie</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> fr(int wid, Guid cc, string referer, string path, string wvri, string r, string b,
            int sw, int sh, int st, int sl, string occ)
        {
            VisitPage? vp = null;
            try
            {
                //get the website 
                var w = websiteRepository.GetWebsiteByID(wid);

                if (w != null)
                {

                    Visit? v = visitRepository.GetVisitByCC(cc);
                    var u = new Uri(path);

                    Webpage? wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                    if (wp == null)
                    {
                        wp = new Webpage()
                        {
                            DateCreated = DateTime.UtcNow,
                            Path = u.AbsolutePath.Trim(),
                            QueryString = u.Query.Trim(),
                            Status = RecordStatus.Active,
                            Website = w
                        };
                        webpageRepository.InsertWebpage(wp);
                        webpageRepository.Save();
                    }
                    //if visit is not found than create a new visit
                    if (v == null)
                    {
                        int? oldVisitID = null;
                        if (!string.IsNullOrEmpty(occ))
                        {
                            if (Guid.TryParse(occ, out Guid oldcc))
                            {
                                var oldVisit = visitRepository.GetVisitByCC(oldcc);
                                if (oldVisit != null)
                                {
                                    oldVisitID = oldVisit.ID;
                                }
                            }
                        }
                        var result = new IP2LocationResult();
                        try
                        {
                            if (Request.HttpContext.Connection.RemoteIpAddress != null)
                                result = await iPLocationWorker.GetLocationAsync(Request.HttpContext.Connection.RemoteIpAddress.ToString());

                        }
                        catch (Exception ex)
                        {
                            result = new IP2LocationResult();
                            logger.LogError(ex, "RVController > fr > GetLocation");
                        }

                        v = new Visit()
                        {
                            CountryAbbr = !string.IsNullOrEmpty(result.Country_Code) ? result.Country_Code : string.Empty,
                            CityName = !string.IsNullOrEmpty(result.City_Name) ? result.City_Name : string.Empty,
                            CountryName = !string.IsNullOrEmpty(result.Country_Name) ? result.Country_Name : string.Empty,
                            IsProxy = result.Is_Proxy,
                            Latitude = result.Latitude.HasValue ? result.Latitude.Value.ToString() : string.Empty,
                            Longitude = result.Longitude.HasValue ? result.Longitude.Value.ToString() : string.Empty,
                            RegionName = result.Region_Name,
                            ZipCode = result.Zip_Code,
                            DateCreated = DateTime.UtcNow,
                            DateModified = null,
                            IPAddress = Request.HttpContext.Connection.RemoteIpAddress != null ? Request.HttpContext.Connection.RemoteIpAddress.ToString() : string.Empty,
                            LastPingDate = DateTime.UtcNow,
                            Referer = !string.IsNullOrEmpty(referer) ? referer : string.Empty,
                            Status = RecordStatus.Active,
                            WebsiteVisitorReferenceID = !string.IsNullOrEmpty(wvri) ? wvri : string.Empty,
                            ClientCookie = cc,
                            Website = w,
                            BrowserName = !string.IsNullOrEmpty(b) ? b : string.Empty,
                            ScreenHeight = sh,
                            ScreenWidth = sw,
                            LastVisitID = oldVisitID
                        };
                        visitRepository.InsertVisit(v);
                        visitRepository.Save();
                    }
                    if (v != null)
                    {
                        vp = visitRepository.GetLastVisitedPageofVisit(v.ID);
                    }
                    //if visitpage is not found or visitpage does not match with webpage in current context
                    //than create a new visitpage obj
                    if (vp == null || vp.webpage.ID != wp.ID)
                    {
                        vp = new VisitPage()
                        {
                            LastPingDate = DateTime.UtcNow,
                            visit = v,
                            webpage = wp,
                            DateCreated = DateTime.UtcNow,
                            BrowserWidth = null,
                            BrowserHeight = null,
                            ScrollLeft = sl,
                            ScrollTop = st
                        };
                        visitRepository.InsertVisitPage(vp);
                        visitRepository.Save();
                    }

                }
            }catch(Exception exception)
            {
                logger.LogError(exception, "RVController > fr");
            }
            return base.File("~/images/trans.png", "image/png");
        }

        /// <summary>
        /// Record Click
        /// </summary>
        /// <param name="id">ID to randomize request</param>
        /// <param name="cc">Client Cookie</param>
        /// <param name="path">Webpage path</param>
        /// <param name="x">Mouse.PageX</param>
        /// <param name="y">Mouse.PageY</param>
        /// <param name="tag">Click tag name</param>
        /// <param name="tagid">Click Tag ID</param>
        /// <returns></returns>
        public ActionResult rc(string id, Guid cc, string path, int x, int y, string tag, string tagid)
        {
            Visit? v = visitRepository.GetVisitByCC(cc);
            if (v != null)
            {
                v.LastPingDate = DateTime.UtcNow;
                visitRepository.UpdateVisit(v);
                visitRepository.Save();

                var u = new Uri(path);
                Webpage? wp = webpageRepository.GetWebpage(v.Website.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    VisitPage? vp = visitRepository.GetLastVisitedPageofVisit(v.ID);

                    if (vp != null && vp.webpage.ID == wp.ID)
                    {
                        var vpa = new VisitActivity
                        {
                            Activity = ActivityName.Click,
                            ClickTagId = !string.IsNullOrEmpty(tagid) ? tagid : string.Empty,
                            ClickTagName = !string.IsNullOrEmpty(tag) ? tag : string.Empty,
                            DateCreated = DateTime.UtcNow,
                            MouseClickX = x,
                            MouseClickY = y,
                            visit = v,
                            visitpage = vp
                        };
                        visitRepository.InsertVisitPageActivity(vpa);
                        visitRepository.Save();
                    }
                }
            }
            return base.File("~/images/trans.png", "image/png");
        }

        /// <summary>
        /// Window Blurred
        /// </summary>
        /// <param name="id">ID to randomize request</param>
        /// <param name="cc">Client Cookie</param>
        /// <param name="path">Webpage path</param>
        /// <returns></returns>
        public ActionResult wb(string id, Guid cc, string path, int s)
        {
            Visit? v = visitRepository.GetVisitByCC(cc);
            if (v != null)
            {
                v.LastPingDate = DateTime.UtcNow;
                visitRepository.UpdateVisit(v);
                visitRepository.Save();

                var u = new Uri(path);
                Webpage? wp = webpageRepository.GetWebpage(v.Website.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    VisitPage? vp = visitRepository.GetLastVisitedPageofVisit(v.ID);

                    if (vp != null && vp.webpage.ID == wp.ID)
                    {
                        var vpa = new VisitActivity
                        {
                            Activity = ActivityName.WindowBlur,
                            ClickTagId = "",
                            ClickTagName = "",
                            DateCreated = DateTime.UtcNow,
                            MouseClickX = null,
                            MouseClickY = null,
                            visit = v,
                            SecondsPassed = s,
                            visitpage = vp
                        };
                        visitRepository.InsertVisitPageActivity(vpa);
                        visitRepository.Save();
                    }
                }
            }
            return base.File("~/images/trans.png", "image/png");
        }

        /// <summary>
        /// Window Focus
        /// </summary>
        /// <param name="id">ID to random request</param>
        /// <param name="cc">Client Cookie</param>
        /// <param name="path">Webpage path</param>
        /// <returns></returns>
        public ActionResult wf(string id, Guid cc, string path, int s)
        {
            Visit? v = visitRepository.GetVisitByCC(cc);
            if (v != null)
            {
                v.LastPingDate = DateTime.UtcNow;
                visitRepository.UpdateVisit(v);
                visitRepository.Save();

                var u = new Uri(path);
                Webpage? wp = webpageRepository.GetWebpage(v.Website.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    VisitPage? vp = visitRepository.GetLastVisitedPageofVisit(v.ID);

                    if (vp != null && vp.webpage.ID == wp.ID)
                    {
                        var vpa = new VisitActivity();
                        vpa.Activity = ActivityName.WindowFocus;
                        vpa.ClickTagId = "";
                        vpa.ClickTagName = "";
                        vpa.DateCreated = DateTime.UtcNow;
                        vpa.MouseClickX = null;
                        vpa.MouseClickY = null;
                        vpa.visit = v;
                        vpa.SecondsPassed = s;
                        vpa.visitpage = vp;
                        visitRepository.InsertVisitPageActivity(vpa);
                        visitRepository.Save();
                    }
                }
            }
            return base.File("~/images/trans.png", "image/png");
        }

        /// <summary>
        /// Scroll Bottom of the page
        /// </summary>
        /// <param name="id">ID to random request</param>
        /// <param name="cc">Client Cookie</param>
        /// <param name="path">Webpage path</param>
        /// <returns></returns>
        public ActionResult sb(string id, Guid cc, string path)
        {
            Visit? v = visitRepository.GetVisitByCC(cc);
            if (v != null)
            {
                v.LastPingDate = DateTime.UtcNow;
                visitRepository.UpdateVisit(v);
                visitRepository.Save();

                var u = new Uri(path);
                Webpage? wp = webpageRepository.GetWebpage(v.Website.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    VisitPage? vp = visitRepository.GetLastVisitedPageofVisit(v.ID);

                    if (vp != null && vp.webpage.ID == wp.ID)
                    {
                        var vpa = new VisitActivity();
                        vpa.Activity = ActivityName.ScrollBottom;
                        vpa.ClickTagId = "";
                        vpa.ClickTagName = "";
                        vpa.DateCreated = DateTime.UtcNow;
                        vpa.MouseClickX = null;
                        vpa.MouseClickY = null;
                        vpa.visit = v;
                        vpa.visitpage = vp;
                        visitRepository.InsertVisitPageActivity(vpa);
                        visitRepository.Save();
                    }
                }
            }
            return base.File("~/images/trans.png", "image/png");

        }

        /// <summary>
        /// Get Javascript to start recording
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns></returns>
        public ContentResult getjs(int id)
        {
            Website? w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                bool load = false;
                var header = Request.GetTypedHeaders();
               

                //check if the script should be loaded,
                //If referer URL is from same website than 
                //only load the script
                if (header.Referer != null)
                {
                    Uri uriReferer = header.Referer;
                    if (uriReferer.Host.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase) || uriReferer.Host.ToLower().EndsWith(string.Format(".{0}", w.Name.ToLower())))
                    {
                        load = true;
                    }
                }
                if (load)
                {
                    string result = System.IO.File.ReadAllText(Path.Combine(webHostEnvironment.WebRootPath, "vt.js"));
                    result = result.Replace("var vt_guid = '';", string.Format("var vt_guid = '{0}';", Guid.NewGuid().ToString()));
                    return Content(result);
                }
            }

            return Content("");
        }
    }

    //public class JavaScriptResult : ActionResult
    //{
    //    //
    //    // Summary:
    //    //     Gets or sets the script.
    //    //
    //    // Returns:
    //    //     The script.
    //    public string Script { get; set; }
    //    public override async void ExecuteResult(ActionContext context)
    //    {
    //        ArgumentNullException.ThrowIfNull(context);

    //        var response = context.HttpContext.Response;
    //        response.ContentType = "application/x-javascript";
    //        if (Script != null)
    //        {
    //            await response.WriteAsync(Script);
    //        }
    //    }
    //    //
    //    // Summary:
    //    //     Enables processing of the result of an action method by a custom type that inherits
    //    //     from the System.Web.Mvc.ActionResult class.
    //    //
    //    // Parameters:
    //    //   context:
    //    //     The context within which the result is executed.
    //    //
    //    // Exceptions:
    //    //   T:System.ArgumentNullException:
    //    //     The context parameter is null.

    //    //
    //    // Summary:
    //    //     Initializes a new instance of the System.Web.Mvc.JavaScriptResult class.
    //    public JavaScriptResult()
    //    {
    //    }
    //}
}


