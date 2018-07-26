using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTracker.DAL;
using VTracker.Models;

namespace VTracker.Controllers
{
    public class RecordVisitController : Controller
    {
        private IWebsiteRepository websiteRepository;
        private IVisitRepository visitRepository;
        private IWebpageRepository webpageRepository;
        private VisitTrackerContext context;

        public RecordVisitController()
        {
            context = new VisitTrackerContext();
            this.websiteRepository = new WebsiteRepository(context);
            this.visitRepository = new VisitRepository(context);
            this.webpageRepository = new WebpageRepository(context);
        }

        public RecordVisitController(IWebsiteRepository repository, IVisitRepository vrepository)
        {
            this.websiteRepository = repository;
            this.visitRepository = vrepository;
        }

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
                Visit v = visitRepository.GetVisitByCC(cc);
                VisitPage vp = null;
                Webpage wp = null;
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
            return base.File(Server.MapPath("~/content/trans.png"), "image/png");
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
        public ActionResult fr(int wid, Guid cc, string referer, string path, string wvri, string r, string b,
            int sw, int sh, int st, int sl, string occ)
        {

            Visit v = null;
            VisitPage vp = null;
            Webpage wp = null;

            //get the website 
            Website w = websiteRepository.GetWebsiteByID(wid);

            if (w != null)
            {

                v = visitRepository.GetVisitByCC(cc);
                Uri u = new Uri(path);

                wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
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
                    Guid oldcc;
                    if (!string.IsNullOrEmpty(occ))
                    {
                        if (Guid.TryParse(occ, out oldcc))
                        {
                            Visit oldVisit = visitRepository.GetVisitByCC(oldcc);
                            if (oldVisit != null)
                            {
                                oldVisitID = oldVisit.ID;
                            }
                        }
                    }
                    v = new Visit()
                    {
                        CountryAbbr = "",
                        DateCreated = DateTime.UtcNow,
                        DateModified = null,
                        IPAddress = Request.UserHostAddress,
                        LastPingDate = DateTime.UtcNow,
                        Referer = referer,
                        Status = RecordStatus.Active,
                        WebsiteVisitorReferenceID = wvri,
                        ClientCookie = cc,
                        Website = w,
                        BrowserName = b,
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

            return base.File(Server.MapPath("~/content/trans.png"), "image/png");
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
            Visit v = visitRepository.GetVisitByCC(cc);
            VisitPage vp = null;
            Webpage wp = null;
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
                        VisitActivity vpa = new VisitActivity();
                        vpa.Activity = ActivityName.Click;
                        vpa.ClickTagId = tagid;
                        vpa.ClickTagName = tag;
                        vpa.DateCreated = DateTime.UtcNow;
                        vpa.MouseClickX = x;
                        vpa.MouseClickY = y;
                        vpa.visit = v;
                        vpa.visitpage = vp;
                        visitRepository.InsertVisitPageActivity(vpa);
                        visitRepository.Save();
                    }
                }
            }
            return base.File(Server.MapPath("~/content/trans.png"), "image/png");
        }

        /// <summary>
        /// Window Blurred
        /// </summary>
        /// <param name="id">ID to randomize request</param>
        /// <param name="cc">Client Cookie</param>
        /// <param name="path">Webpage path</param>
        /// <returns></returns>
        public ActionResult wb(string id, Guid cc, string path, int s) {
            Visit v = visitRepository.GetVisitByCC(cc);
            VisitPage vp = null;
            Webpage wp = null;
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
                        VisitActivity vpa = new VisitActivity();
                        vpa.Activity = ActivityName.WindowBlur;
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
            return base.File(Server.MapPath("~/content/trans.png"), "image/png");
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
            Visit v = visitRepository.GetVisitByCC(cc);
            VisitPage vp = null;
            Webpage wp = null;
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
                        VisitActivity vpa = new VisitActivity();
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
            return base.File(Server.MapPath("~/content/trans.png"), "image/png");
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
            Visit v = visitRepository.GetVisitByCC(cc);
            VisitPage vp = null;
            Webpage wp = null;
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
                        VisitActivity vpa = new VisitActivity();
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
            return base.File(Server.MapPath("~/content/trans.png"), "image/png");

        }

        /// <summary>
        /// Get Javascript to start recording
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns></returns>
        public JavaScriptResult getjs(int id)
        {
            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                bool load = false;
                //check if the script should be loaded,
                //If referer URL is from same website than 
                //only load the script
                if (Request.UrlReferrer != null)
                {
                    if (Request.UrlReferrer.Host.ToLower() == w.Name.ToLower() || Request.UrlReferrer.Host.ToLower().EndsWith(string.Format(".{0}", w.Name.ToLower())))
                    {
                        load = true;
                    }
                }
                if (load)
                {
                    string result = System.IO.File.ReadAllText(Server.MapPath("~/vt.js"));
                    result = result.Replace("var vt_guid = '';", string.Format("var vt_guid = '{0}';", Guid.NewGuid().ToString()));
                    return JavaScript(result);
                }
            }

            return JavaScript("");
        }
        protected override void Dispose(bool disposing)
        {
            websiteRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}