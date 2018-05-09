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
    public class WebageDisplayPublic
    {
        public string Path { get; set; }
        public int VisitLastHour { get; set; }
        public int VisitToday { get; set; }
        public int VisitLastWeek { get; set; }
        public int VisitLast30Days { get; set; }
        public int TotalVisits { get; set; }

    }
}