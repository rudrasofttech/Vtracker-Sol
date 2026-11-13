using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class VisitPage
    {
        public int ID { get; set; }
        public virtual Visit visit { get; set; }
        public virtual Webpage webpage { get; set; }
        public DateTime LastPingDate { get; set; }
        public DateTime DateCreated { get; set; }

        public int? BrowserWidth { get; set; }
        public int? BrowserHeight { get; set; }
        public int? ScrollTop { get; set; }
        public int? ScrollLeft { get; set; }

        public byte? MaxScrollPct { get; set; }
        public int? DocumentHeight { get; set; }
    }
}
