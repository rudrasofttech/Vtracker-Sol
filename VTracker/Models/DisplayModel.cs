﻿using System;
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
        Months = 3
    }

    public class WebpageDisplayPublic
    {

        public WebpageDisplayPublic()
        {
            VisitsData = new List<VisitCountChartPoint>();
            BrowserData = new List<PieChartPoint>();
            RefererList = new List<RefererData>();
            NewVisitsData = new List<VisitCountChartPoint>();
            ReturnVisitsData = new List<VisitCountChartPoint>();
        }
        public int WebsiteId { get; set; }
        public string Path { get; set; }
        public ReportDateRangeType Range { get; set; }
        public int VisitCount { get; set; }
        public List<VisitCountChartPoint> VisitsData { get; set; }
        public List<VisitCountChartPoint> NewVisitsData { get; set; }
        public List<VisitCountChartPoint> ReturnVisitsData { get; set; }
        public List<PieChartPoint> BrowserData { get; set; }
        public string VisitChartDescription { get; set; }
        public List<RefererData> RefererList { get; set; }

    }

    public class WebsiteDisplayPublic
    {
        public WebsiteDisplayPublic()
        {
            VisitsData = new List<VisitCountChartPoint>();
            BrowserData = new List<PieChartPoint>();
            RefererList = new List<RefererData>();
            NewVisitsData = new List<VisitCountChartPoint>();
            ReturnVisitsData = new List<VisitCountChartPoint>();
            MPages = new List<WebpageVisits>();
            WebsiteName = string.Empty;
        }
        public string WebsiteName { get; set; }
        public int WebsiteId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public ReportDateRangeCustomType Range { get; set; }
        public int VisitCount { get; set; }
        public List<VisitCountChartPoint> VisitsData { get; set; }
        public List<VisitCountChartPoint> NewVisitsData { get; set; }
        public List<VisitCountChartPoint> ReturnVisitsData { get; set; }
        public List<PieChartPoint> BrowserData { get; set; }
        public string VisitChartDescription { get; set; }
        public List<RefererData> RefererList { get; set; }
        //most visited pages
        public List<WebpageVisits> MPages { get; set; }
    }
    public struct WebpageVisits
    {
        public string Page { get; set; }
        public int Count { get; set; }
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

    public class RefererData
    {
        public string Referer { get; set; }
        public int Count { get; set; }
    }
}