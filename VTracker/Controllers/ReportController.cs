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


        public ActionResult WebpagePublicStats(int id, string path, DateTime? start, DateTime? end)
        {
            if (!start.HasValue)
            {
                start = new DateTime(DateTime.UtcNow.AddDays(-15).Year, DateTime.UtcNow.AddDays(-15).Month, DateTime.UtcNow.AddDays(-15).Day);
            }
            if (!end.HasValue)
            {
                end = DateTime.UtcNow;
            }
            WebpageDisplayPublic wdp = new WebpageDisplayPublic();
            wdp.Path = path;
            wdp.WebsiteId = id;
            wdp.Start = start.Value;
            wdp.End = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
            if (wdp.Start.Year == wdp.End.Year && wdp.Start.Month == wdp.End.Month && wdp.Start.Day == wdp.End.Day)
            {
                wdp.Range = ReportDateRangeCustomType.Day;
            }
            else if (wdp.End.Subtract(wdp.Start).TotalDays < 60)
            {
                wdp.Range = ReportDateRangeCustomType.Days;
            }
            else if (wdp.End.Subtract(wdp.Start).TotalDays > 60)
            {
                wdp.Range = ReportDateRangeCustomType.Months;
            }
            wdp.VisitChartDescription = string.Format("Visits from {0} till {1}", wdp.Start.ToString("d/MMM/yy"), end);
            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                wdp.WebsiteName = w.Name;
                Uri u = new Uri(path);
                Webpage wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    var visits = visitRepository.GetVisitsByWebpage(wp.ID, start, end);
                    var visitact = visitRepository.GetVisitPageActivities(visits.Select(t => t.ID).ToList(), wp.ID);


                    wdp.BrowserData.AddRange(GetBrowserUsage(visits.ToList()));
                    wdp.VisitCount = visits.ToList().Count;
                    DateTime current = DateTime.UtcNow;
                    Dictionary<string, int> browsers = new Dictionary<string, int>();
                    foreach (var item in visits)
                    {
                        string refstr = "";

                        if (!string.IsNullOrEmpty(item.Referer))
                        {
                            Uri referurl = new Uri(HttpUtility.UrlDecode(item.Referer));
                            refstr = referurl.Host;
                        }



                        RefererData rd = wdp.RefererList.SingleOrDefault(t => t.Referer == refstr);
                        if (rd != null)
                        {
                            rd.Count++;
                        }
                        else
                        {
                            wdp.RefererList.Add(new RefererData() { Count = 1, Referer = refstr });
                        }

                    }
                    DateTime daystart = wdp.Start;
                    switch (wdp.Range)
                    {
                        case ReportDateRangeCustomType.Day:
                            while (daystart <= wdp.End)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("h:m tt");
                                p.Y = visits.Where(t => t.DateCreated >= daystart &&
                                t.DateCreated <= daystart.AddHours(1)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("h:m tt");
                                np.Y = visits.Where(t => t.DateCreated >= daystart &&
                                t.DateCreated <= daystart.AddHours(1) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("h:m tt");
                                rp.Y = visits.Where(t => t.DateCreated >= daystart &&
                                t.DateCreated <= daystart.AddHours(1) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddHours(1);
                            }


                            break;


                        case ReportDateRangeCustomType.Days:

                            while (daystart <= wdp.End)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.ToString("MM/d");
                                p.Y = visits.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.ToString("MM/d");
                                np.Y = visits.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.ToString("MM/d");
                                rp.Y = visits.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(1);
                            }


                            break;
                        case ReportDateRangeCustomType.Months:


                            while (daystart <= wdp.End)
                            {
                                VisitCountChartPoint p = new VisitCountChartPoint();
                                p.X = daystart.AddDays(15).ToString("y-M-d");
                                p.Y = visits.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15)).Count();
                                wdp.VisitsData.Add(p);

                                VisitCountChartPoint np = new VisitCountChartPoint();
                                np.X = daystart.AddDays(15).ToString("y-M-d");
                                np.Y = visits.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == false).Count();
                                wdp.NewVisitsData.Add(np);

                                VisitCountChartPoint rp = new VisitCountChartPoint();
                                rp.X = daystart.AddDays(15).ToString("y-M-d");
                                rp.Y = visits.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == true).Count();
                                wdp.ReturnVisitsData.Add(rp);

                                daystart = daystart.AddDays(15);
                            }

                            break;
                    }
                    HeatMapGenerator hmg = new HeatMapGenerator(Server.MapPath("~/Content/img/palette.bmp"));
                    hmg.ws = w;
                    hmg.wp = wp;
                    hmg.Activities = visitact.Where(t => t.Activity == ActivityName.Click).ToList();
                    hmg.GenerateMap();
                    wdp.HeatMapPath = hmg.HeatMapPath;
                }


            }


            return View(wdp);
        }

        public ActionResult WebsitePublicStats(int id, DateTime? start, DateTime? end)
        {
            if (!start.HasValue)
            {
                start = new DateTime(DateTime.UtcNow.AddDays(-15).Year, DateTime.UtcNow.AddDays(-15).Month, DateTime.UtcNow.AddDays(-15).Day);
            }
            if (!end.HasValue)
            {
                end = DateTime.UtcNow;
            }
            WebsiteDisplayPublic wdp = new WebsiteDisplayPublic();
            wdp.WebsiteId = id;
            wdp.Start = start.Value;
            wdp.End = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
            if (wdp.Start.Year == wdp.End.Year && wdp.Start.Month == wdp.End.Month && wdp.Start.Day == wdp.End.Day)
            {
                wdp.Range = ReportDateRangeCustomType.Day;
            }
            else if (wdp.End.Subtract(wdp.Start).TotalDays < 60)
            {
                wdp.Range = ReportDateRangeCustomType.Days;
            }
            else if (wdp.End.Subtract(wdp.Start).TotalDays > 60)
            {
                wdp.Range = ReportDateRangeCustomType.Months;
            }
            wdp.VisitChartDescription = string.Format("Visits from {0} till {1}", wdp.Start.ToString("d/MMM/yy"), end);
            Website w = websiteRepository.GetWebsiteByID(id);
            if (w != null)
            {
                wdp.WebsiteName = w.Name;
                var visits = visitRepository.GetVisits(id, wdp.Start, wdp.End);
                var visitpages = visitRepository.GetVisitAndWebpageByWebsite(id, wdp.Start, wdp.End);
                var pages = webpageRepository.GetWebpages(w.ID);

                foreach (var p in pages)
                {
                    wdp.MPages.Add(new WebpageVisits() { Count = visitpages.Count(t => t.Item2.ID == p.ID), Page = p.Path });
                }
                Dictionary<string, int> browsers = new Dictionary<string, int>();
                var vt = visits.Where(t => t.DateCreated >= wdp.Start && t.DateCreated <= wdp.End).ToList();
                wdp.VisitCount = vt.Count;

                foreach (var item in vt)
                {
                    string refstr = "";
                    string screens = "";
                    if (!string.IsNullOrEmpty(item.Referer))
                    {
                        Uri referurl = new Uri(HttpUtility.UrlDecode(item.Referer));
                        refstr = referurl.Host;
                    }

                    if (item.ScreenWidth != null && item.ScreenHeight != null)
                    {
                        screens = string.Format("{0} X {1}", item.ScreenWidth, item.ScreenHeight);
                    }

                    RefererData rd = wdp.RefererList.SingleOrDefault(t => t.Referer == refstr);
                    if (rd != null)
                    {
                        rd.Count++;
                    }
                    else
                    {
                        wdp.RefererList.Add(new RefererData() { Count = 1, Referer = refstr });
                    }

                    ScreenSizeData ssd = wdp.ScreenSizes.SingleOrDefault(t => t.Screen == screens);
                    if (ssd != null)
                    {
                        ssd.Count++;
                    }
                    else
                    {
                        wdp.ScreenSizes.Add(new ScreenSizeData() { Count = 1, Screen = screens });
                    }
                }
                DateTime daystart = wdp.Start;
                switch (wdp.Range)
                {
                    case ReportDateRangeCustomType.Day:
                        while (daystart <= wdp.End)
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

                        wdp.BrowserData.AddRange(GetBrowserUsage(vt));
                        break;


                    case ReportDateRangeCustomType.Days:

                        while (daystart <= wdp.End)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.ToString("MM/d");
                            p.Y = vt.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.ToString("MM/d");
                            np.Y = vt.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.ToString("MM/d");
                            rp.Y = vt.Where(t => t.DateCreated.Year == daystart.Year && t.DateCreated.Month == daystart.Month && t.DateCreated.Day == daystart.Day && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(1);
                        }

                        wdp.BrowserData.AddRange(GetBrowserUsage(vt));
                        break;
                    case ReportDateRangeCustomType.Months:


                        while (daystart <= wdp.End)
                        {
                            VisitCountChartPoint p = new VisitCountChartPoint();
                            p.X = daystart.AddDays(15).ToString("y-M-d");
                            p.Y = vt.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15)).Count();
                            wdp.VisitsData.Add(p);

                            VisitCountChartPoint np = new VisitCountChartPoint();
                            np.X = daystart.AddDays(15).ToString("y-M-d");
                            np.Y = vt.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == false).Count();
                            wdp.NewVisitsData.Add(np);

                            VisitCountChartPoint rp = new VisitCountChartPoint();
                            rp.X = daystart.AddDays(15).ToString("y-M-d");
                            rp.Y = vt.Where(t => t.DateCreated >= daystart && t.DateCreated < daystart.AddDays(15) && t.LastVisitID.HasValue == true).Count();
                            wdp.ReturnVisitsData.Add(rp);

                            daystart = daystart.AddDays(15);
                        }
                        wdp.BrowserData.AddRange(GetBrowserUsage(vt));
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