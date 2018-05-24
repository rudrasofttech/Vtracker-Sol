using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTracker.Models
{

    public class Utility
    {
        //seconds
        public const int PulseInterval = 5;
    }
    public class WebpageDisplayPublic
    {

        public WebpageDisplayPublic()
        {
            VisitsLastHour = new List<VisitCountChartPoint>();
            VisitsLastMonth = new List<VisitCountChartPoint>();
            VisitsLastWeek = new List<VisitCountChartPoint>();
            VisitsToday = new List<VisitCountChartPoint>();
            VisitsTotal = new List<VisitCountChartPoint>();
        }
        public string Path { get; set; }
        public int VisitCountLast30Days { get; set; }
        public int VisitCountLastHour { get; set; }
        public int VisitCountLastWeek { get; set; }
        public int VisitCountToday { get; set; }
        public int TotalVisitCount { get; set; }
        public List<VisitCountChartPoint> VisitsLastHour { get; set; }
        public List<VisitCountChartPoint> VisitsToday { get; set; }
        public List<VisitCountChartPoint> VisitsLastWeek { get; set; }
        public List<VisitCountChartPoint> VisitsLastMonth { get; set; }
        public List<VisitCountChartPoint> VisitsTotal { get; set; }

    }

    public class VisitCountChartPoint
    {
        public string X { get; set; }
        public int Y { get; set; }
    }
}