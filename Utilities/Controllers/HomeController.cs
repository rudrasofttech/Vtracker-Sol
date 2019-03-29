using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities.Models;

namespace Utilities.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public JsonResult WebToImage(string url)
        {
            string image = string.Format("~/temp.jpeg");
            WebsiteToImage wti = new WebsiteToImage(url, Server.MapPath(image));
            wti.Generate();
            return Json(new { image = Url.Content(image) }, JsonRequestBehavior.AllowGet);
        }

        
    }
}
