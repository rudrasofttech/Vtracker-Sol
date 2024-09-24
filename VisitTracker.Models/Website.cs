using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class Website
    {
        public int ID { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public int? ClientID { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public RecordStatus Status { get; set; }

        public virtual ICollection<Webpage> Webpages { get; set; }
    }
}
