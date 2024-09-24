using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Web;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Pages
{
    public class PageReportModel(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment) : PageModel
    {
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly VisitManager visitRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);
        private readonly VisitTrackerDBContext context = _context;
        private readonly IWebHostEnvironment webHostEnvironment = _webHostEnvironment;

        [FromQuery(Name = "id")]
        public int Id { get; set; }
        [FromQuery(Name = "start")]
        public DateTime? Start { get; set; }
        [FromQuery(Name = "end")]
        public DateTime? End { get; set; }

        [FromQuery(Name = "path")]
        public string Path { get; set; }

        public WebpageDisplayPublic DisplayInfo { get; set; } = new();

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

            DisplayInfo.Path = Path;
            DisplayInfo.WebsiteId = Id;
            DisplayInfo.Start = Start.Value;
            DisplayInfo.End = new DateTime(End.Value.Year, End.Value.Month, End.Value.Day, 23, 59, 59);

            if (DisplayInfo.Start.Year == DisplayInfo.End.Year && DisplayInfo.Start.Month == DisplayInfo.End.Month && DisplayInfo.Start.Day == DisplayInfo.End.Day)
            {
                DisplayInfo.Range = ReportDateRangeCustomType.Day;
            }
            else if (DisplayInfo.End.Subtract(DisplayInfo.Start).TotalDays < 60)
            {
                DisplayInfo.Range = ReportDateRangeCustomType.Days;
            }
            else if (DisplayInfo.End.Subtract(DisplayInfo.Start).TotalDays > 60)
            {
                DisplayInfo.Range = ReportDateRangeCustomType.Months;
            }
            DisplayInfo.VisitChartDescription = string.Format("Visits from {0} till {1}", DisplayInfo.Start.ToString("d/MMM/yy"), End);
            var w = websiteRepository.GetWebsiteByID(Id);
            if (w != null)
            {
                DisplayInfo.WebsiteName = w.Name;
                var u = new Uri(Path);
                var wp = webpageRepository.GetWebpage(w.ID, u.AbsolutePath.Trim(), u.Query.Trim());
                if (wp != null)
                {
                    var visits = visitRepository.GetVisitsByWebpage(wp.ID, Start, End);
                    var visitAct = visitRepository.GetVisitPageActivities(visits.Select(t => t.ID).ToList(), wp.ID);


                    DisplayInfo.BrowserData.AddRange(GetBrowserUsage(visits.ToList()));
                    DisplayInfo.VisitCount = visits.ToList().Count;
                    DateTime current = DateTime.UtcNow;
                    Dictionary<string, int> browsers = new Dictionary<string, int>();
                    foreach (var item in visits)
                    {
                        string refstr = "";

                        if (!string.IsNullOrEmpty(item.Referer))
                        {
                            var referurl = new Uri(HttpUtility.UrlDecode(item.Referer));
                            refstr = referurl.Host;
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
                                    Y = visits.Where(t => t.DateCreated >= dayStart &&
                                    t.DateCreated <= dayStart.AddHours(1)).Count()
                                };
                                DisplayInfo.VisitsData.Add(p);

                                var np = new VisitCountChartPoint
                                {
                                    X = dayStart.ToString("h:m tt"),
                                    Y = visits.Where(t => t.DateCreated >= dayStart &&
                                    t.DateCreated <= dayStart.AddHours(1) && t.LastVisitID.HasValue == false).Count()
                                };
                                DisplayInfo.NewVisitsData.Add(np);

                                var rp = new VisitCountChartPoint
                                {
                                    X = dayStart.ToString("h:m tt"),
                                    Y = visits.Where(t => t.DateCreated >= dayStart &&
                                    t.DateCreated <= dayStart.AddHours(1) && t.LastVisitID.HasValue == true).Count()
                                };
                                DisplayInfo.ReturnVisitsData.Add(rp);

                                dayStart = dayStart.AddHours(1);
                            }
                            break;


                        case ReportDateRangeCustomType.Days:

                            while (dayStart <= DisplayInfo.End)
                            {
                                var p = new VisitCountChartPoint
                                {
                                    X = dayStart.ToString("MM/d"),
                                    Y = visits.Where(t => t.DateCreated.Year == dayStart.Year && t.DateCreated.Month == dayStart.Month && t.DateCreated.Day == dayStart.Day).Count()
                                };
                                DisplayInfo.VisitsData.Add(p);

                                var np = new VisitCountChartPoint
                                {
                                    X = dayStart.ToString("MM/d"),
                                    Y = visits.Where(t => t.DateCreated.Year == dayStart.Year && t.DateCreated.Month == dayStart.Month && t.DateCreated.Day == dayStart.Day && t.LastVisitID.HasValue == false).Count()
                                };
                                DisplayInfo.NewVisitsData.Add(np);

                                var rp = new VisitCountChartPoint
                                {
                                    X = dayStart.ToString("MM/d"),
                                    Y = visits.Where(t => t.DateCreated.Year == dayStart.Year && t.DateCreated.Month == dayStart.Month && t.DateCreated.Day == dayStart.Day && t.LastVisitID.HasValue == true).Count()
                                };
                                DisplayInfo.ReturnVisitsData.Add(rp);

                                dayStart = dayStart.AddDays(1);
                            }


                            break;
                        case ReportDateRangeCustomType.Months:


                            while (dayStart <= DisplayInfo.End)
                            {
                                var p = new VisitCountChartPoint
                                {
                                    X = dayStart.AddDays(15).ToString("y-M-d"),
                                    Y = visits.Where(t => t.DateCreated >= dayStart && t.DateCreated < dayStart.AddDays(15)).Count()
                                };
                                DisplayInfo.VisitsData.Add(p);

                                var np = new VisitCountChartPoint
                                {
                                    X = dayStart.AddDays(15).ToString("y-M-d"),
                                    Y = visits.Where(t => t.DateCreated >= dayStart && t.DateCreated < dayStart.AddDays(15) && t.LastVisitID.HasValue == false).Count()
                                };
                                DisplayInfo.NewVisitsData.Add(np);

                                var rp = new VisitCountChartPoint
                                {
                                    X = dayStart.AddDays(15).ToString("y-M-d"),
                                    Y = visits.Where(t => t.DateCreated >= dayStart && t.DateCreated < dayStart.AddDays(15) && t.LastVisitID.HasValue == true).Count()
                                };
                                DisplayInfo.ReturnVisitsData.Add(rp);

                                dayStart = dayStart.AddDays(15);
                            }

                            break;
                    }
                    //var hmg = new HeatMapGenerator($"{webHostEnvironment.WebRootPath}/Content/img/palette.bmp", webHostEnvironment.WebRootPath);
                    //hmg.ws = w;
                    //hmg.wp = wp;
                    //hmg.Activities = visitAct.Where(t => t.Activity == ActivityName.Click).ToList();
                    //hmg.GenerateMap();
                    //DisplayInfo.HeatMapPath = hmg.HeatMapPath;
                }
            }
        }

        private List<PieChartPoint> GetBrowserUsage(List<Visit> vt)
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
