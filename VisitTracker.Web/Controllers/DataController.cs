using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitTracker.DataContext;
using VisitTracker.Models;

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
    }
}
