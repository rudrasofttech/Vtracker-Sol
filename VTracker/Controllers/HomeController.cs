using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VTracker.Models;

namespace VTracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public async Task<string> AddUser()
        {
            ApplicationUser user;
            ApplicationUserStore Store = new ApplicationUserStore(new ApplicationDbContext());
            ApplicationUserManager userManager = new ApplicationUserManager(Store);
            user = new ApplicationUser
            {
                UserName = "rajkiransingh",
                Email = "rajkiran.singh@gmail.com"
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return result.Errors.First();
            }
            return "User Added";
        }
    }
}
