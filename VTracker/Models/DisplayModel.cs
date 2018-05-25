using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VTracker.Models
{

    public class Utility
    {
        //seconds
        public const int PulseInterval = 5;

        public static string GetWebpagePublicStatsURL(int websiteid, string path, ReportDateRangeType rangeType)
        {
            return string.Format("~/report/webpagepublicstats/{0}?path={1}&range={2}", websiteid, path, rangeType);
        }
    }

    public enum ReportDateRangeType
    {
        LastHour = 1,
        Today = 2,
        Last7Days = 3,
        Last15Days = 4,
        Last30Days = 5,
        Last3Months = 6,
        Last6Months = 7,
        Last12Months = 12
    }
    public class WebpageDisplayPublic
    {

        public WebpageDisplayPublic()
        {
            VisitsData = new List<VisitCountChartPoint>();
            BrowserData = new List<PieChartPoint>();
        }
        public int WebsiteId { get; set; }
        public string Path { get; set; }
        public ReportDateRangeType Range { get; set; }
        //public int VisitCountLast30Days { get; set; }
        //public int VisitCountLastHour { get; set; }
        //public int VisitCountLastWeek { get; set; }
        //public int VisitCountToday { get; set; }
        //public int TotalVisitCount { get; set; }
        public int VisitCount { get; set; }
        public List<VisitCountChartPoint> VisitsData { get; set; }
        public List<PieChartPoint> BrowserData { get; set; }
        public string VisitChartDescription { get; set; }

    }

    public class VisitCountChartPoint
    {
        public string X { get; set; }
        public int Y { get; set; }
    }

    public class PieChartPoint
    {
        public string Label { get; set; }
        public int Percentage { get; set; }
    }
}