using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VisitTracker.Models
{

    public class Utility
    {
        //seconds
        public const int PulseInterval = 5;

        public static string GetWebpagePublicStatsURL(int websiteId, string path)
        {
            return string.Format("~/report/webpagepublicstats/{0}?path={1}", websiteId, path);
        }

        public static string GetWebsitePublicStatsURL(int websiteid, ReportDateRangeType rangeType)
        {
            return string.Format("~/report/websitepublicstats/{0}?range={1}", websiteid, rangeType);
        }
    }

    public enum ReportDateRangeType
    {
        LastHour = 1,
        Today = 2,
        Yesterday = 13,
        Last7Days = 3,
        Last15Days = 4,
        Last30Days = 5,
        Last3Months = 6,
        Last6Months = 7,
        Last12Months = 12
    }

    public enum ReportDateRangeCustomType
    {
        Day = 1,
        Days = 2,
        Months = 3,
        None
    }

    public class WebpageDisplayPublic
    {

        public string WebsiteName { get; set; } = string.Empty;
        public string HeatMapPath { get; set; } = string.Empty;
        public int WebsiteId { get; set; }
        public string Path { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int VisitCount { get; set; }
        public ReportDateRangeCustomType Range { get; set; }
        public List<VisitCountChartPoint> VisitsData { get; set; } = [];
        public List<VisitCountChartPoint> NewVisitsData { get; set; } = [];
        public List<VisitCountChartPoint> ReturnVisitsData { get; set; } = [];
        public List<PieChartPoint> BrowserData { get; set; } = [];
        public string VisitChartDescription { get; set; } = string.Empty;
        public List<RefererData> RefererList { get; set; } = [];

    }

    public class WebsiteDisplayPublic(int _websiteId, DateTime _start, DateTime _end)
    {
        public string WebsiteName { get; set; } = string.Empty;
        public int WebsiteId { get; set; } = _websiteId;
        public DateTime Start { get; set; } = _start;
        public DateTime End { get; set; } = _end;
        public ReportDateRangeCustomType Range { get
            {
                if (Start.Year == End.Year && Start.Month == End.Month && Start.Day == End.Day)
                {
                    return ReportDateRangeCustomType.Day;
                }
                else if (End.Subtract(Start).TotalDays < 60)
                {
                    return ReportDateRangeCustomType.Days;
                }
                else if (End.Subtract(Start).TotalDays > 60)
                {
                    return ReportDateRangeCustomType.Months;
                }
                return ReportDateRangeCustomType.None;
            }
        }
        public int VisitCount { get; set; }
        public List<VisitCountChartPoint> VisitsData { get; set; } = [];
        public List<VisitCountChartPoint> NewVisitsData { get; set; } = [];
        public List<VisitCountChartPoint> ReturnVisitsData { get; set; } = [];
        public List<PieChartPoint> BrowserData { get; set; } = [];
        public string VisitChartDescription { get { return string.Format("Visits from {0} till {1}", Start.ToString("d/MMM/yy"), End); } }
        public List<RefererData> RefererList { get; set; } = [];
        public List<ScreenSizeData> ScreenSizes { get; set; } = [];
        //most visited pages
        public List<WebpageVisits> MPages { get; set; } = [];
    }

    public struct WebpageVisits
    {
        public string AbsoluteUrl { get;set; }
        public string Page { get; set; }
        public int Count { get; set; }
    }
    public class VisitCountChartPoint
    {
        public string X { get; set; } = string.Empty;
        public int Y { get; set; }
    }

    public class PieChartPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Percentage { get; set; }
    }

    public class RefererData
    {
        public string Referer { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ScreenSizeData
    {
        public string Screen { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class HomepageData
    {
        public HomepageData()
        {
            Stats = new List<WebsiteStats>();
        }
        public List<WebsiteStats> Stats { get; set; } = [];
        public int VisitCount { get; set; }
        public int ActivityCount { get; set; }
        public int WebsiteCount { get; set; }
        public int WebpageCount { get; set; }
    }

    public class WebsiteStats
    {
        public Website Site { get; set; }
        public int VisitCount { get; set; }
        public int ActivityCount { get; set; }
    }
}