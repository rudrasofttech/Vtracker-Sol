using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class Webpage
    {
        [Column(Order = 1)]
        public int ID { get; set; }
        [Column(Order = 2), Required, MaxLength(1024)]
        public string Path { get; set; }
        [Column(Order = 3), Required(AllowEmptyStrings = true)]
        public string QueryString { get; set; }
        [Column(Order = 4)]
        public DateTime DateCreated { get; set; }
        [Column(Order = 5)]
        public DateTime? DateModified { get; set; }
        [Column(Order = 6)]
        public RecordStatus Status { get; set; }
        [Column(Order = 7)]
        [Required]
        public virtual Website Website { get; set; }
    }
}
