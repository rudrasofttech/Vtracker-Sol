using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VisitTracker.DataContext;
using VisitTracker.Models;
using VisitTracker.Models.DTO;

namespace VisitTracker.Web.Controllers
{
    
    
    public class DataController(VisitTrackerDBContext _context, IWebHostEnvironment _webHostEnvironment, IConfiguration config, ILogger<DataController> _logger) : Controller
    {
        private readonly WebsiteManager websiteRepository = new(_context);
        private readonly VisitManager visitRepository = new(_context);
        private readonly WebpageManager webpageRepository = new(_context);
        private readonly IWebHostEnvironment webHostEnvironment = _webHostEnvironment;
        private readonly IPLocationWorker iPLocationWorker = new(config, _context);
        private readonly ILogger<DataController> logger = _logger;

        [HttpGet]
        public ActionResult ViewCount([FromQuery]int websiteId, [FromQuery] string path)
        {
            try
            {
                return Ok(webpageRepository.GetVisitCount(websiteId, path));
            }
            catch (Exception ex) {

                logger.LogError(ex, "DataController > ViewCount");
                return StatusCode(500, new { error = "Unable to process request", exception = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult AddWebsite([FromBody] AddWebsiteModel model)
        {
            try {
                var wb = websiteRepository.GetWebsiteByName(model.Name);
                if(wb != null)
                    return BadRequest(new { error = "Website already exists." });

                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new { error = "Website name missing." });

                wb = new Website()
                {
                    Name = model.Name.ToLower(),
                    Status = RecordStatus.Active,
                    DateCreated = DateTime.UtcNow,
                    OwnerId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value)
                };
                websiteRepository.InsertWebsite(wb);
                websiteRepository.Save();
                string script = "var vt_root = 'https://www.webstats.co.in/'; var vt_website_id = '" + wb.ID + "'; var vt_wvri = ''; var VTInit = (function () { function VTInit() { }  VTInit.prototype.initialize = function () { var seed = document.createElement(\"script\"); seed.setAttribute(\"src\", vt_root + \"rv/getjs/\" + vt_website_id);\r\n if (document.getElementsByTagName(\"head\").length > 0) {\r\n   document.getElementsByTagName(\"head\")[0].appendChild(seed);\r\n                }\r\n            };\r\n            return VTInit;\r\n        }());\r\n        var _vtInit = new VTInit();\r\n        _vtInit.initialize();";
                return Ok(new { message = "Website added.", websiteId = wb.ID, script });
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "DataController > AddWebsite");
                return StatusCode(500, new { error = "Unable to process request" });
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult GetScript([FromQuery]string name)
        {
            try
            {
                var wb = websiteRepository.GetWebsiteByName(name);
                if (wb == null)
                    return BadRequest(new { error = "Website does not exists." });
                string script = "var vt_root = 'https://www.webstats.co.in/'; var vt_website_id = '" + wb.ID + "'; var vt_wvri = ''; var VTInit = (function () { function VTInit() { }  VTInit.prototype.initialize = function () { var seed = document.createElement(\"script\"); seed.setAttribute(\"src\", vt_root + \"rv/getjs/\" + vt_website_id);\r\n if (document.getElementsByTagName(\"head\").length > 0) {\r\n   document.getElementsByTagName(\"head\")[0].appendChild(seed);\r\n                }\r\n            };\r\n            return VTInit;\r\n        }());\r\n        var _vtInit = new VTInit();\r\n        _vtInit.initialize();";
                return Ok(new { script });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataController > GetScript");
                return StatusCode(500, new { error = "Unable to process request" });
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult RemoveWebsite([FromBody] RemoveWebsiteModel model)
        {
            try
            {
                if(ModelState.IsValid == false)
                    return BadRequest(new { error = "Invalid request." });
                var wb = websiteRepository.GetWebsiteByName(model.Name);
                if (wb == null)
                    return NotFound(new { error = "Website not found." });
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                if (wb.OwnerId != userId && !User.IsInRole("Admin"))
                    return Forbid();
                websiteRepository.DeleteWebsite(wb.ID);
                websiteRepository.Save();
                return Ok(new { message = "Website removed." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataController > RemoveWebsite");
                return StatusCode(500, new { error = "Unable to process request" });
            }
        }

    }
}
