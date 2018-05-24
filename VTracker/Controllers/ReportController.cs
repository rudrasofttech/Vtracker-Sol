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

        //[OutputCache(Duration = 60, VaryByParam = "path")]
        public ActionResult WebpagePublicStats(int id, string path) {
            WebpageDisplayPublic wdp = new WebpageDisplayPublic();

            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                Uri u = new Uri(path);
                Webpage wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    var visits = visitRepository.GetVisitsByWebpage(wp.ID).Distinct();
                    //visits = visits.Where(t => t.LastPingDate.Subtract(t.DateCreated).Seconds >= Utility.PulseInterval);
                    wdp.Path = path;

                    wdp.VisitCountLast30Days = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-30)).Count();
                    wdp.VisitCountLastHour = visits.Where(t => t.DateCreated >= DateTime.Now.AddMinutes(-60)).Count();
                    wdp.VisitCountLastWeek = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-7)).Count();
                    wdp.VisitCountToday = visits.Where(t => t.DateCreated >= DateTime.Now.AddHours(-1 * DateTime.Now.Hour)).Count();
                    wdp.TotalVisitCount = visits.Count();

                    //visit last hour
                    var vlh = visits.Where(t => t.DateCreated >= DateTime.Now.AddMinutes(-60)).ToList();
                    DateTime past60min = DateTime.Now.AddMinutes(-60);
                    DateTime current = DateTime.Now;
                    while(past60min <= current)
                    {
                        VisitCountChartPoint p = new VisitCountChartPoint();
                        p.X = past60min.ToString("hh:mm tt");
                        p.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(10)
                        ).Count();
                        wdp.VisitsLastHour.Add(p);
                        past60min = past60min.AddMinutes(10);
                    }

                    //visit today
                    var vt = visits.Where(t => t.DateCreated >= DateTime.Now.AddHours(-1 * DateTime.Now.Hour)).ToList();
                    DateTime daystart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    current = DateTime.Now;
                    while(daystart <= current)
                    {
                        VisitCountChartPoint p = new VisitCountChartPoint();
                        p.X = daystart.ToString("hh:mm tt");
                        p.Y = vt.Where(t => t.DateCreated >= daystart &&
                        t.DateCreated <= daystart.AddHours(1)).Count();
                        wdp.VisitsToday.Add(p);

                        daystart = daystart.AddHours(1);
                    }

                    ////visit last seven days
                    var vw = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-7)).ToList();
                    daystart = new DateTime(DateTime.Now.AddDays(-7).Year, DateTime.Now.AddDays(-7).Month, DateTime.Now.AddDays(-7).Day);
                    current = DateTime.Now;
                    while(daystart <= current)
                    {
                        VisitCountChartPoint p = new VisitCountChartPoint();
                        p.X = daystart.ToString("M/d");
                        p.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                        wdp.VisitsLastWeek.Add(p);

                        daystart = daystart.AddDays(1);
                    }

                    //visit this month
                    var vm = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-30)).ToList();
                    daystart = new DateTime(DateTime.Now.AddDays(-30).Year, DateTime.Now.AddDays(-30).Month, DateTime.Now.AddDays(-30).Day);
                    current = DateTime.Now;
                    while (daystart <= current)
                    {
                        VisitCountChartPoint p = new VisitCountChartPoint();
                        p.X = daystart.ToString("M/d");
                        p.Y = vm.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                        wdp.VisitsLastMonth.Add(p);

                        daystart = daystart.AddDays(1);
                    }

                    //visit this month
                    //var vto = visits.ToList();
                    //daystart = vto.Count > 0 ? vto[0].DateCreated.AddDays(-1) : DateTime.Now;
                    //current = DateTime.Now;
                    //while (daystart <= current)
                    //{
                    //    VisitCountChartPoint p = new VisitCountChartPoint();
                    //    p.X = daystart.ToString("M/d");
                    //    p.Y = vm.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                    //    wdp.VisitsLastMonth.Add(p);

                    //    daystart = daystart.AddDays(1);
                    //}
                }
            }
            return View(wdp);
        }

    }
}