using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class VisitActivity
    {
        public int ID { get; set; }
        public virtual VisitPage visitpage { get; set; }
        public virtual Visit visit { get; set; }

        public ActivityName Activity { get; set; }
        public DateTime DateCreated { get; set; }
        public int? MouseClickX { get; set; }
        public int? MouseClickY { get; set; }
        [MaxLength(50)]
        public string ClickTagName { get; set; }
        /// <summary>
        /// This will store tag id or cssclass or innertext, depending upon what is found
        /// </summary>
        [MaxLength(200)]
        public string ClickTagId { get; set; }

        public int? SecondsPassed { get; set; }
    }
}
