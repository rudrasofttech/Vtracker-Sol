using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Web;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Pages
{
    public class ReportModel(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment) : PageModel
    {
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly VisitManager visitRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);

        [FromQuery(Name = "id")]
        public int Id { get; set; }
        [FromQuery(Name = "start")]
        public DateTime? Start { get; set; }
        [FromQuery(Name = "end")]
        public DateTime? End { get; set; }
        public WebsiteDisplayPublic DisplayInfo { get; set; }
        public void OnGet()
        {

            if (!Start.HasValue)
            {
                Start = new DateTime(DateTime.UtcNow.AddDays(-15).Year, DateTime.UtcNow.AddDays(-15).Month, DateTime.UtcNow.AddDays(-15).Day);
            }
            if (!End.HasValue)
            {
                End = DateTime.UtcNow;
            }
            DisplayInfo = new(Id, Start.Value, new DateTime(End.Value.Year, End.Value.Month, End.Value.Day, 23, 59, 59));

            
            var w = websiteRepository.GetWebsiteByID(Id);
            if (w != null)
            {
                DisplayInfo.WebsiteName = w.Name;
                var visits = visitRepository.GetVisits(Id, DisplayInfo.Start, DisplayInfo.End);
                var visitPages = visitRepository.GetVisitAndWebpageByWebsite(Id, DisplayInfo.Start, DisplayInfo.End);
                var pages = webpageRepository.GetWebpages(w.ID);

                foreach (var p in pages)
                {
                    DisplayInfo.MPages.Add(new WebpageVisits() { Count = visitPages.Count(t => t.Item2.ID == p.ID), Page = p.Path, AbsoluteUrl = $"https://{w.Name}{p.Path}" });
                }
                var browsers = new Dictionary<string, int>();
                var vt = visits.Where(t => t.DateCreated >= DisplayInfo.Start && t.DateCreated <= DisplayInfo.End).ToList();
                DisplayInfo.VisitCount = vt.Count;

                foreach (var item in vt)
                {
                    string refstr = "";
                    string screens = "";
                    if (!string.IsNullOrEmpty(item.Referer))
                    {
                        var referurl = new Uri(HttpUtility.UrlDecode(item.Referer));
                        refstr = referurl.Host;
                    }

                    if (item.ScreenWidth != null && item.ScreenHeight != null)
                    {
                        screens = string.Format("{0} X {1}", item.ScreenWidth, item.ScreenHeight);
                    }

                    var rd = DisplayInfo.RefererList.SingleOrDefault(t => t.Referer == refstr);
                    if (rd != null)
                    {
                        rd.Count++;
                    }
                    else
                    {
                        DisplayInfo.RefererList.Add(new RefererData() { Count = 1, Referer = refstr });
                    }

                    var ssd = DisplayInfo.ScreenSizes.SingleOrDefault(t => t.Screen == screens);
                    if (ssd != null)
                    {
                        ssd.Count++;
                    }
                    else
                    {
                        DisplayInfo.ScreenSizes.Add(new ScreenSizeData() { Count = 1, Screen = screens });
                    }
                }
                var dayStart = DisplayInfo.Start;
                switch (DisplayInfo.Range)
                {
                    case ReportDateRangeCustomType.Day:
                        while (dayStart <= DisplayInfo.End)
                        {
                            var p = new VisitCountChartPoint
                            {
                                X = dayStart.ToString("h:m tt"),
                                Y = vt.Where(t => t.DateCreated >= dayStart &&
                                t.DateCreated <= dayStart.AddHours(1)).Count()
                            };
                            DisplayInfo.VisitsData.Add(p);

                            var np = new VisitCountChartPoint
                            {
                                X = dayStart.ToString("h:m tt"),
                                Y = vt.Where(t => t.DateCreated >= dayStart &&
                                t.DateCreated <= dayStart.AddHours(1) && t.LastVisitID.HasValue == false).Count()
                            };
                            DisplayInfo.NewVisitsData.Add(np);

                            var rp = new VisitCountChartPoint
                            {
                                X = dayStart.ToString("h:m tt"),
                                Y = vt.Where(t => t.DateCreated >= dayStart &&
                                t.DateCreated <= dayStart.AddHours(1) && t.LastVisitID.HasValue == true).Count()
                            };
                            DisplayInfo.ReturnVisitsData.Add(rp);

                            dayStart = dayStart.AddHours(1);
                        }

                        DisplayInfo.BrowserData.AddRange(GetBrowserUsage(vt));
                        break;


                    case ReportDateRangeCustomType.Days:

                        while (dayStart <= DisplayInfo.End)
                        {
                            var p = new VisitCountChartPoint
                            {
                                X = dayStart.ToString("MM/d"),
                                Y = vt.Where(t => t.DateCreated.Year == dayStart.Year && t.DateCreated.Month == dayStart.Month && t.DateCreated.Day == dayStart.Day).Count()
                            };
                            DisplayInfo.VisitsData.Add(p);

                            var np = new VisitCountChartPoint
                            {
                                X = dayStart.ToString("MM/d"),
                                Y = vt.Where(t => t.DateCreated.Year == dayStart.Year && t.DateCreated.Month == dayStart.Month && t.DateCreated.Day == dayStart.Day && t.LastVisitID.HasValue == false).Count()
                            };
                            DisplayInfo.NewVisitsData.Add(np);

                            var rp = new VisitCountChartPoint
                            {
                                X = dayStart.ToString("MM/d"),
                                Y = vt.Where(t => t.DateCreated.Year == dayStart.Year && t.DateCreated.Month == dayStart.Month && t.DateCreated.Day == dayStart.Day && t.LastVisitID.HasValue == true).Count()
                            };
                            DisplayInfo.ReturnVisitsData.Add(rp);

                            dayStart = dayStart.AddDays(1);
                        }

                        DisplayInfo.BrowserData.AddRange(GetBrowserUsage(vt));
                        break;
                    case ReportDateRangeCustomType.Months:


                        while (dayStart <= DisplayInfo.End)
                        {
                            var p = new VisitCountChartPoint
                            {
                                X = dayStart.AddDays(15).ToString("y-M-d"),
                                Y = vt.Where(t => t.DateCreated >= dayStart && t.DateCreated < dayStart.AddDays(15)).Count()
                            };
                            DisplayInfo.VisitsData.Add(p);

                            var np = new VisitCountChartPoint
                            {
                                X = dayStart.AddDays(15).ToString("y-M-d"),
                                Y = vt.Where(t => t.DateCreated >= dayStart && t.DateCreated < dayStart.AddDays(15) && t.LastVisitID.HasValue == false).Count()
                            };
                            DisplayInfo.NewVisitsData.Add(np);

                            var rp = new VisitCountChartPoint
                            {
                                X = dayStart.AddDays(15).ToString("y-M-d"),
                                Y = vt.Where(t => t.DateCreated >= dayStart && t.DateCreated < dayStart.AddDays(15) && t.LastVisitID.HasValue == true).Count()
                            };
                            DisplayInfo.ReturnVisitsData.Add(rp);

                            dayStart = dayStart.AddDays(15);
                        }
                        DisplayInfo.BrowserData.AddRange(GetBrowserUsage(vt));
                        break;
                }
            }
        }

        List<PieChartPoint> GetBrowserUsage(List<Visit> vt)
        {
            var result = new List<PieChartPoint>
            {
                new() { Label = "Chrome", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("chrome ")).Count() },
                new() { Label = "Firefox", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("firefox ")).Count() },
                new() { Label = "Safari", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("safari ")).Count() },
                new() { Label = "Opera", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("opera ")).Count() },
                new() { Label = "Edge", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("edge ")).Count() },
                new() { Label = "IE", Percentage = vt.Where(t => t.BrowserName != null && t.BrowserName.ToLower().StartsWith("microsoft internet explorer ")).Count() },
                new()
                {
                    Label = "Other",
                    Percentage = vt.Where(t => t.BrowserName != null && !t.BrowserName.ToLower().StartsWith("chrome ") &&
    !t.BrowserName.ToLower().StartsWith("firefox ") &&
    !t.BrowserName.ToLower().StartsWith("safari ") &&
    !t.BrowserName.ToLower().StartsWith("opera ") &&
    !t.BrowserName.ToLower().StartsWith("edge ") &&
    !t.BrowserName.ToLower().StartsWith("microsoft internet explorer ")).Count()
                }
            };

            return result;
        }
    }
}
