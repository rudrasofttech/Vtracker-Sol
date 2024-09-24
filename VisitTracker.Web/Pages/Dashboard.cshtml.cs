using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VisitTracker.Web.Pages
{
    

    public class DashboardModel : PageModel
    {
        [FromQuery(Name = "id")]
        public int Id { get; set; }
        public void OnGet()
        {
        }
    }
}
