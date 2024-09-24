using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class Visit
    {
        [Key]
        public int ID { get; set; }
        [Required, MaxLength(20)]
        public string IPAddress { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; }
        public RecordStatus Status { get; set; }
        [Required(AllowEmptyStrings = true), MaxLength(5)]
        public string CountryAbbr { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        public string Referer { get; set; }

        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public required string WebsiteVisitorReferenceID { get; set; } = string.Empty;

        public Guid ClientCookie { get; set; }

        public DateTime LastPingDate { get; set; }
        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string? BrowserName { get; set; } = string.Empty;

        public int? ScreenWidth { get; set; }
        public int? ScreenHeight { get; set; }

        public int? LastVisitID { get; set; }

        public virtual ICollection<VisitPage> VisitPages { get; set; }
        public virtual Website Website { get; set; }
        public string? CountryName { get; set; } = string.Empty;
        public string? RegionName { get; set; } = string.Empty;
        public string? CityName { get; set; } = string.Empty;
        public string? Latitude { get; set; } = string.Empty;
        public string? Longitude { get; set; } = string.Empty;
        public string? ZipCode { get; set; } = string.Empty;
        public bool? IsProxy { get; set; } = false;
    }
}
