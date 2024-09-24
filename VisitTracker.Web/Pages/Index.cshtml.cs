using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Pages
{
    public class IndexModel(ILogger<IndexModel> logger, VisitTrackerDBContext context) : PageModel
    {
        private readonly ILogger<IndexModel> _logger = logger;
        private readonly WebsiteManager websiteRepository = new(context);
        private readonly VisitManager visitRepository = new(context);
        private readonly WebpageManager webpageRepository = new(context);

        public List<WebsiteStats> Stats { get; set; } = [];
        public int VisitCount { get; set; }
        public int ActivityCount { get; set; }
        public int WebsiteCount { get; set; }
        public int WebpageCount { get; set; }

        public void OnGet()
        {
            //_logger.LogInformation($"Serilog Working - {DateTime.Now}");
            //foreach (var ws in websiteRepository.GetWebsites([RecordStatus.Active]))
            //{
            //    Stats.Add(new WebsiteStats()
            //    {
            //        ActivityCount = visitRepository.GetActivityCount(ws.ID),
            //        Site = ws,
            //        VisitCount = visitRepository.GetVisitCount(ws.ID)
            //    });
            //}
            //WebsiteCount = websiteRepository.GetWebsiteCount([RecordStatus.Active]);
            //WebpageCount = webpageRepository.GetWebpageCount([RecordStatus.Active]);
            //VisitCount = visitRepository.GetVisitCount();
            //ActivityCount = visitRepository.GetActivityCount();
        }
    }
}
