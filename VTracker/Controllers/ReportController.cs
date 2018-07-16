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
                    count = visitRepository.GetVisitsByWebpage(wp.ID).Count();
                }
            }
            return Content(count.ToString());
        }

        //[OutputCache(Duration = 60, VaryByParam = "path")]
        public ActionResult WebpagePublicStats(int id, string path, ReportDateRangeType range = ReportDateRangeType.Today)
        {
            WebpageDisplayPublic wdp = new WebpageDisplayPublic();
            wdp.Path = path;
            wdp.WebsiteId = id;
            wdp.Range = range;

            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                Uri u = new Uri(path);
                Webpage wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    var visits = visitRepository.GetVisitsByWebpage(wp.ID);
                    DateTime current = DateTime.Now;
                    Dictionary<string, int> browsers = new Dictionary<string, int>();
                    switch (wdp.Range)
                    {
                        case ReportDateRangeType.Today:
                            var vt = visits.Where(t => t.DateCreated >= DateTime.Now.AddHours(-1 * DateTime.Now.Hour)).ToList();
                            DateTime daystart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                            wdp.VisitCount = vt.Count;
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("h:m tt");
                                p.Y = vt.Where(t => t.DateCreated >= daystart &&
                                t.DateCreated <= daystart.AddHours(1)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("h:m tt");
                                np.Y = vt.Where(t => t.DateCreated >= daystart &&
                                t.DateCreated <= daystart.AddHours(1) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("h:m tt");
                                rp.Y = vt.Where(t => t.DateCreated >= daystart &&
                                t.DateCreated <= daystart.AddHours(1) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddHours(1);
                            }
                            wdp.VisitChartDescription = "Today";
                            wdp.BrowserData.AddRange(GetBrowserUsage(vt));
                            wdp.RefererList.AddRange(vt.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Yesterday:
                            var vty = visits.Where(t => t.DateCreated >= new DateTime(DateTime.Now.AddDays(-1).Year, DateTime.Now.AddDays(-1).Month, DateTime.Now.AddDays(-1).Day)
                            && t.DateCreated < new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                            ).ToList();
                            DateTime daystarty = new DateTime(DateTime.Now.AddDays(-1).Year, DateTime.Now.AddDays(-1).Month, DateTime.Now.AddDays(-1).Day);
                            wdp.VisitCount = vty.Count;
                            while (daystarty <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystarty.ToString("h:m tt");
                                p.Y = vty.Where(t => t.DateCreated >= daystarty &&
                                t.DateCreated <= daystarty.AddHours(1)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystarty.ToString("h:m tt");
                                np.Y = vty.Where(t => t.DateCreated >= daystarty &&
                                t.DateCreated <= daystarty.AddHours(1) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystarty.ToString("h:m tt");
                                rp.Y = vty.Where(t => t.DateCreated >= daystarty &&
                                t.DateCreated <= daystarty.AddHours(1) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystarty = daystarty.AddHours(1);
                            }
                            wdp.VisitChartDescription = "Yesterday";
                            wdp.BrowserData.AddRange(GetBrowserUsage(vty));
                            wdp.RefererList.AddRange(vty.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.LastHour:
                            var vlh = visits.Where(t => t.DateCreated >= DateTime.Now.AddMinutes(-60)).ToList();
                            wdp.VisitCount = vlh.Count;
                            DateTime past60min = DateTime.Now.AddMinutes(-60);
                            while (past60min <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = past60min.ToString("h:m tt");
                                p.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(5)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = past60min.ToString("h:m tt");
                                np.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(5) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = past60min.ToString("h:m tt");
                                rp.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(5) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                past60min = past60min.AddMinutes(5);
                            }
                            wdp.VisitChartDescription = "Last 60 Minutes";
                            wdp.BrowserData.AddRange(GetBrowserUsage(vlh));
                            wdp.RefererList.AddRange(vlh.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Last7Days:
                            var vw = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-7)).ToList();
                            wdp.VisitCount = vw.Count;
                            daystart = new DateTime(DateTime.Now.AddDays(-7).Year, DateTime.Now.AddDays(-7).Month, DateTime.Now.AddDays(-7).Day);
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("MM/d");
                                p.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("MM/d");
                                np.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("MM/d");
                                rp.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(1);
                            }
                            wdp.VisitChartDescription = "Last 7 Days";
                            wdp.BrowserData.AddRange(GetBrowserUsage(vw));
                            wdp.RefererList.AddRange(vw.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Last6Months:
                            var vm = visits.Where(t => t.DateCreated >= DateTime.Now.AddMonths(-6)).ToList();
                            wdp.VisitCount = vm.Count;
                            daystart = new DateTime(DateTime.Now.AddMonths(-6).Year, DateTime.Now.AddMonths(-6).Month, 1);
                            current = DateTime.Now;
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.AddDays(15).ToString("y-M-d");
                                p.Y = vm.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.AddDays(15).ToString("y-M-d");
                                np.Y = vm.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.AddDays(15).ToString("y-M-d");
                                rp.Y = vm.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(15);
                            }
                            wdp.VisitChartDescription = "Last 6 Months";
                            wdp.BrowserData.AddRange(GetBrowserUsage(vm));
                            wdp.RefererList.AddRange(vm.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Last3Months:
                            var v3m = visits.Where(t => t.DateCreated >= DateTime.Now.AddMonths(-3)).ToList();
                            wdp.VisitCount = v3m.Count;
                            daystart = new DateTime(DateTime.Now.AddMonths(-3).Year, DateTime.Now.AddMonths(-3).Month, 1);
                            current = DateTime.Now;
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.AddDays(15).ToString("y-M-d");
                                p.Y = v3m.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.AddDays(15).ToString("y-M-d");
                                np.Y = v3m.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.AddDays(15).ToString("y-M-d");
                                rp.Y = v3m.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(15);
                            }
                            wdp.VisitChartDescription = "Last 3 Months";
                            wdp.BrowserData.AddRange(GetBrowserUsage(v3m));
                            wdp.RefererList.AddRange(v3m.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Last30Days:
                            var v30d = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-30)).ToList();
                            wdp.VisitCount = v30d.Count;
                            daystart = new DateTime(DateTime.Now.AddDays(-30).Year, DateTime.Now.AddDays(-30).Month, DateTime.Now.AddDays(-30).Day);
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("MM/d");
                                p.Y = v30d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("MM/d");
                                np.Y = v30d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("MM/d");
                                rp.Y = v30d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(1);
                            }
                            wdp.VisitChartDescription = "Last 30 Days";
                            wdp.BrowserData.AddRange(GetBrowserUsage(v30d));
                            wdp.RefererList.AddRange(v30d.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Last15Days:

                            var v15d = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-15)).ToList();
                            wdp.VisitCount = v15d.Count;
                            daystart = new DateTime(DateTime.Now.AddDays(-15).Year, DateTime.Now.AddDays(-15).Month, DateTime.Now.AddDays(-15).Day);
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("MM/d");
                                p.Y = v15d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("MM/d");
                                np.Y = v15d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("MM/d");
                                rp.Y = v15d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(1);
                            }
                            wdp.VisitChartDescription = "Last 15 Days";
                            wdp.BrowserData.AddRange(GetBrowserUsage(v15d));
                            wdp.RefererList.AddRange(v15d.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                        case ReportDateRangeType.Last12Months:
                            var v12m = visits.Where(t => t.DateCreated >= DateTime.Now.AddMonths(-12)).ToList();
                            wdp.VisitCount = v12m.Count;
                            daystart = new DateTime(DateTime.Now.AddMonths(-12).Year, DateTime.Now.AddMonths(-12).Month, 1);
                            current = DateTime.Now;
                            while (daystart <= current)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("y MMM");
                                p.Y = v12m.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("y MMM");
                                np.Y = v12m.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("y MMM");
                                rp.Y = v12m.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddMonths(1);
                            }
                            wdp.VisitChartDescription = "Last 12 Months";
                            wdp.BrowserData.AddRange(GetBrowserUsage(v12m));
                            wdp.RefererList.AddRange(v12m.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                            break;
                    }
                }
            }
            return View(wdp);
        }

        public ActionResult WebsitePublicStats(int id, ReportDateRangeType range = ReportDateRangeType.Today)
        {
            WebsiteDisplayPublic wdp = new WebsiteDisplayPublic();
            wdp.WebsiteId = id;
            wdp.Range = range;

            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {

                var visits = visitRepository.GetVisits(id);
                DateTime current = DateTime.Now;
                Dictionary<string, int> browsers = new Dictionary<string, int>();
                switch (wdp.Range)
                {
                    case ReportDateRangeType.Today:
                        var vt = visits.Where(t => t.DateCreated >= DateTime.Now.AddHours(-1 * DateTime.Now.Hour)).ToList();
                        DateTime daystart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        wdp.VisitCount = vt.Count;
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.ToString("h:m tt");
                            p.Y = vt.Where(t => t.DateCreated >= daystart &&
                            t.DateCreated <= daystart.AddHours(1)).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.ToString("h:m tt");
                            np.Y = vt.Where(t => t.DateCreated >= daystart &&
                            t.DateCreated <= daystart.AddHours(1) && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.ToString("h:m tt");
                            rp.Y = vt.Where(t => t.DateCreated >= daystart &&
                            t.DateCreated <= daystart.AddHours(1) && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddHours(1);
                        }
                        wdp.VisitChartDescription = "Today";
                        wdp.BrowserData.AddRange(GetBrowserUsage(vt));
                        wdp.RefererList.AddRange(vt.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Yesterday:
                        var vty = visits.Where(t => t.DateCreated >= new DateTime(DateTime.Now.AddDays(-1).Year, DateTime.Now.AddDays(-1).Month, DateTime.Now.AddDays(-1).Day)
                        && t.DateCreated < new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                        ).ToList();
                        DateTime daystarty = new DateTime(DateTime.Now.AddDays(-1).Year, DateTime.Now.AddDays(-1).Month, DateTime.Now.AddDays(-1).Day);
                        wdp.VisitCount = vty.Count;
                        while (daystarty <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystarty.ToString("h:m tt");
                            p.Y = vty.Where(t => t.DateCreated >= daystarty &&
                            t.DateCreated <= daystarty.AddHours(1)).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystarty.ToString("h:m tt");
                            np.Y = vty.Where(t => t.DateCreated >= daystarty &&
                            t.DateCreated <= daystarty.AddHours(1) && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystarty.ToString("h:m tt");
                            rp.Y = vty.Where(t => t.DateCreated >= daystarty &&
                            t.DateCreated <= daystarty.AddHours(1) && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystarty = daystarty.AddHours(1);
                        }
                        wdp.VisitChartDescription = "Yesterday";
                        wdp.BrowserData.AddRange(GetBrowserUsage(vty));
                        wdp.RefererList.AddRange(vty.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.LastHour:
                        var vlh = visits.Where(t => t.DateCreated >= DateTime.Now.AddMinutes(-60)).ToList();
                        wdp.VisitCount = vlh.Count;
                        DateTime past60min = DateTime.Now.AddMinutes(-60);
                        while (past60min <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = past60min.ToString("h:m tt");
                            p.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(5)).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = past60min.ToString("h:m tt");
                            np.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(5) && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = past60min.ToString("h:m tt");
                            rp.Y = vlh.Where(t => t.DateCreated >= past60min && t.DateCreated < past60min.AddMinutes(5) && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            past60min = past60min.AddMinutes(5);
                        }
                        wdp.VisitChartDescription = "Last 60 Minutes";
                        wdp.BrowserData.AddRange(GetBrowserUsage(vlh));
                        wdp.RefererList.AddRange(vlh.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Last7Days:
                        var vw = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-7)).ToList();
                        wdp.VisitCount = vw.Count;
                        daystart = new DateTime(DateTime.Now.AddDays(-7).Year, DateTime.Now.AddDays(-7).Month, DateTime.Now.AddDays(-7).Day);
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.ToString("MM/d");
                            p.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.ToString("MM/d");
                            np.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.ToString("MM/d");
                            rp.Y = vw.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(1);
                        }
                        wdp.VisitChartDescription = "Last 7 Days";
                        wdp.BrowserData.AddRange(GetBrowserUsage(vw));
                        wdp.RefererList.AddRange(vw.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Last6Months:
                        var vm = visits.Where(t => t.DateCreated >= DateTime.Now.AddMonths(-6)).ToList();
                        wdp.VisitCount = vm.Count;
                        daystart = new DateTime(DateTime.Now.AddMonths(-6).Year, DateTime.Now.AddMonths(-6).Month, 1);
                        current = DateTime.Now;
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.AddDays(15).ToString("y-M-d");
                            p.Y = vm.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15)).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.AddDays(15).ToString("y-M-d");
                            np.Y = vm.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.AddDays(15).ToString("y-M-d");
                            rp.Y = vm.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(15);
                        }
                        wdp.VisitChartDescription = "Last 6 Months";
                        wdp.BrowserData.AddRange(GetBrowserUsage(vm));
                        wdp.RefererList.AddRange(vm.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Last3Months:
                        var v3m = visits.Where(t => t.DateCreated >= DateTime.Now.AddMonths(-3)).ToList();
                        wdp.VisitCount = v3m.Count;
                        daystart = new DateTime(DateTime.Now.AddMonths(-3).Year, DateTime.Now.AddMonths(-3).Month, 1);
                        current = DateTime.Now;
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.AddDays(15).ToString("y-M-d");
                            p.Y = v3m.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15)).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.AddDays(15).ToString("y-M-d");
                            np.Y = v3m.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.AddDays(15).ToString("y-M-d");
                            rp.Y = v3m.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(15);
                        }
                        wdp.VisitChartDescription = "Last 3 Months";
                        wdp.BrowserData.AddRange(GetBrowserUsage(v3m));
                        wdp.RefererList.AddRange(v3m.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Last30Days:
                        var v30d = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-30)).ToList();
                        wdp.VisitCount = v30d.Count;
                        daystart = new DateTime(DateTime.Now.AddDays(-30).Year, DateTime.Now.AddDays(-30).Month, DateTime.Now.AddDays(-30).Day);
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.ToString("MM/d");
                            p.Y = v30d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.ToString("MM/d");
                            np.Y = v30d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.ToString("MM/d");
                            rp.Y = v30d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(1);
                        }
                        wdp.VisitChartDescription = "Last 30 Days";
                        wdp.BrowserData.AddRange(GetBrowserUsage(v30d));
                        wdp.RefererList.AddRange(v30d.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Last15Days:

                        var v15d = visits.Where(t => t.DateCreated >= DateTime.Now.AddDays(-15)).ToList();
                        wdp.VisitCount = v15d.Count;
                        daystart = new DateTime(DateTime.Now.AddDays(-15).Year, DateTime.Now.AddDays(-15).Month, DateTime.Now.AddDays(-15).Day);
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.ToString("MM/d");
                            p.Y = v15d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.ToString("MM/d");
                            np.Y = v15d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.ToString("MM/d");
                            rp.Y = v15d.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(1);
                        }
                        wdp.VisitChartDescription = "Last 15 Days";
                        wdp.BrowserData.AddRange(GetBrowserUsage(v15d));
                        wdp.RefererList.AddRange(v15d.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;
                    case ReportDateRangeType.Last12Months:
                        var v12m = visits.Where(t => t.DateCreated >= DateTime.Now.AddMonths(-12)).ToList();
                        wdp.VisitCount = v12m.Count;
                        daystart = new DateTime(DateTime.Now.AddMonths(-12).Year, DateTime.Now.AddMonths(-12).Month, 1);
                        current = DateTime.Now;
                        while (daystart <= current)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.ToString("y MMM");
                            p.Y = v12m.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.ToString("y MMM");
                            np.Y = v12m.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.ToString("y MMM");
                            rp.Y = v12m.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddMonths(1);
                        }
                        wdp.VisitChartDescription = "Last 12 Months";
                        wdp.BrowserData.AddRange(GetBrowserUsage(v12m));
                        wdp.RefererList.AddRange(v12m.GroupBy(t => t.Referer).Select(t => new RefererData { Referer = t.Key, Count = t.Count() }).ToList());
                        break;

                }
            }
            return View(wdp);
        }

        public List<PieChartPoint> GetBrowserUsage(List<Visit> vt)
        {
            List<PieChartPoint> result = new List<PieChartPoint>();
            result.Add(new PieChartPoint() { Label = "Chrome", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("chrome ")).Count() });
            result.Add(new PieChartPoint() { Label = "Firefox", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("firefox ")).Count() });
            result.Add(new PieChartPoint() { Label = "Safari", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("safari ")).Count() });
            result.Add(new PieChartPoint() { Label = "Opera", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("opera ")).Count() });
            result.Add(new PieChartPoint() { Label = "Edge", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("edge ")).Count() });
            result.Add(new PieChartPoint() { Label = "IE", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("microsoft internet explorer ")).Count() });
            result.Add(new PieChartPoint()
            {
                Label = "Other",
                Percentage = vt.Where(t => t.BrowserName != null && !t.BrowserName.ToLower().StartsWith("chrome ") &&
!t.BrowserName.ToLower().StartsWith("firefox ") &&
!t.BrowserName.ToLower().StartsWith("safari ") &&
!t.BrowserName.ToLower().StartsWith("opera ") &&
!t.BrowserName.ToLower().StartsWith("edge ") &&
!t.BrowserName.ToLower().StartsWith("microsoft internet explorer ")).Count()
            });

            return result;
        }

    }
}