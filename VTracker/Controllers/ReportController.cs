using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using VTracker.DAL;
using VTracker.Models;

namespace VTracker.Controllers
{
    public class ReportController : Controller
    {
        private IWebsiteRepository websiteRepository;
        private IVisitRepository visitRepository;
        private IWebpageRepository webpageRepository;
        private VisitTrackerContext context;

        public ReportController()
        {
            context = new VisitTrackerContext();
            this.websiteRepository = new WebsiteRepository(context);
            this.visitRepository = new VisitRepository(context);
            this.webpageRepository = new WebpageRepository(context);
        }
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 180, VaryByParam = "path")]
        public ActionResult Count(int id, string path)
        {
            
                int count = 0;
                Website w = websiteRepository.GetWebsiteByID(id);
                if (w != null)
                {
                    Uri u = new Uri(path);
                    Webpage wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                    if (wp != null)
                    {
                        count = visitRepository.GetVisitsByWebpage(wp.ID).Where(t => t.LastPingDate.Subtract(t.DateCreated).Seconds >= Utility.PulseInterval).Count();
                    }
                }
            return Content(count.ToString());
        }

        [OutputCache(Duration = 180, VaryByParam = "path")]
        public ActionResult WebpagePublicStats(int id, string path) {
            WebageDisplayPublic wdp = new WebageDisplayPublic();

            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                Uri u = new Uri(path);
                Webpage wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    var visits = visitRepository.GetVisitsByWebpage(wp.ID);
                    visits = visits.Where(t => t.LastPingDate.Subtract(t.DateCreated).Seconds >= Utility.PulseInterval);
                    wdp.Path = path;
                    
                    wdp.VisitLast30Days = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-30)).Count();
                    wdp.VisitLastHour = visits.Where(t => t.DateCreated >= DateTime.Now.AddMinutes(-60)).Count();
                    wdp.VisitLastWeek = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-7)).Count();
                    wdp.VisitToday = visits.Where(t => t.DateCreated >= DateTime.Now.AddHours(-24)).Count();
                    wdp.TotalVisits = visits.Count();
                }
            }
            return View(wdp);
        }
    }
}