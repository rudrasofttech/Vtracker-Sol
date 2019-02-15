using System.Linq;
using System.Web.Mvc;
using VTracker.DAL;
using VTracker.Models;

namespace VTracker.Controllers
{
    public class HomeController : Controller
    {

        private IWebsiteRepository websiteRepository;
        private IVisitRepository visitRepository;
        private IWebpageRepository webpageRepository;
        private VisitTrackerContext context;

        public HomeController()
        {
            context = new VisitTrackerContext();
            this.websiteRepository = new WebsiteRepository(context);
            this.visitRepository = new VisitRepository(context);
            this.webpageRepository = new WebpageRepository(context);
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            HomepageData model = new HomepageData();
            model.Websites.AddRange(websiteRepository.GetWebsites().ToList());
            model.VisitCount = visitRepository.GetVisitCount();
            model.ActivityCount = visitRepository.GetActivityCount();
            return View(model);
        }

       
    }
}
