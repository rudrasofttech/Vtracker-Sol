using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class VisitCountView
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int WebsiteId { get; set; }
        public int VisitCount { get; set; }
    }
}
