
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Web.Mvc;
using VTracker.Models;

namespace VTracker.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationUserManager userManager;
        private RoleManager<ApplicationRole> roleManager;

        public AccountController()
        {
            ApplicationDbContext dbcontext = ApplicationDbContext.Create();
            userManager = new ApplicationUserManager(new ApplicationUserStore(dbcontext));
            roleManager = new RoleManager<ApplicationRole>(new ApplicationRoleStore(dbcontext));
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                UserStore<ApplicationUser> Store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                ApplicationUserManager userManager = new ApplicationUserManager(Store);

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}