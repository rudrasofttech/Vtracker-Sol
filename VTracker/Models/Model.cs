using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace VTracker.Models
{
    public enum RecordStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }

    public enum ActivityName
    {
        Click = 1
    }

    public class Website
    {
        public int ID { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        public int? ClientID { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public RecordStatus Status { get; set; }

        public virtual ICollection<Webpage> Webpages { get; set; }
    }

    public class Webpage {
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

    public class Visit {
        public int ID { get; set; }
        [Required, MaxLength(20)]
        public string IPAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public RecordStatus Status { get; set; }
        [Required(AllowEmptyStrings = true), MaxLength(5)]
        public string CountryAbbr { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string Referer { get; set; }
        
        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string WebsiteVisitorReferenceID { get; set; }
        
        public Guid ClientCookie { get; set; }
        
        public DateTime LastPingDate { get; set; }
        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string BrowserName { get; set; }

        public int? ScreenWidth { get; set; }
        public int? ScreenHeight { get; set; }

        public int? LastVisitID { get; set; }

        public virtual ICollection<VisitPage> VisitPages { get; set; }
        public virtual Website Website { get; set; }
    }

    public class VisitPage
    {
        public int ID { get; set; }
        public virtual Visit visit { get; set; }
        public virtual Webpage webpage { get; set; }
        public DateTime LastPingDate { get; set; }
        public DateTime DateCreated { get; set; }

        public int? BrowserWidth { get; set; }
        public int? BrowserHeight { get; set; }
        /// <summary>
        /// Initial vertical scroll  position when page is loaded
        /// </summary>
        public int? ScrollTop { get; set; }

        /// <summary>
        /// Initial Horizontal scroll  position when page is loaded
        /// </summary>
        public int? ScrollLeft { get; set; }
        
    }

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
    }

    public class VisitTrackerContext : DbContext
    {
        public DbSet<Website> Websites { get; set; }
        public DbSet<Webpage> Webpages { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<VisitPage> VisitPages { get; set; }
        public DbSet<VisitActivity> VisitPageActivities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}