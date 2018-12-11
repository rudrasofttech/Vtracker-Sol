using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace VTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        //You can extend this class by adding additional fields like Birthday
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("VisitTrackerContext", throwIfV1Schema: false)
        {
        }

    }

    public class OwinAuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public OwinAuthDbContext()
            : base("VisitTrackerContext")
        {
        }
    }

}