using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using VisitTracker.Models;

namespace VisitTracker.DataContext
{
    public class WebpageManager(VisitTrackerDBContext _context)
    {
        private VisitTrackerDBContext context = _context;

        public IEnumerable<Webpage> GetWebpages(int? websiteId)
        {
            return context.Webpages.Where(t => t.Website.ID == websiteId || !websiteId.HasValue).ToList();
        }

        public Webpage? GetWebpageByID(int Id)
        {
            return context.Webpages.FirstOrDefault(t => t.ID == Id);
        }

        public void InsertWebpage(Webpage w)
        {
            context.Webpages.Add(w);
        }

        public int GetWebpageCount(List<RecordStatus> statuses)
        {
            return context.Webpages.Count(t => statuses.Contains(t.Status));
        }

        public void DeleteWebpage(int Id)
        {
            var w = context.Webpages.First(t => t.ID == Id);
            context.Webpages.Remove(w);
        }

        public void UpdateWebpage(Webpage w)
        {
            context.Entry(w).State = EntityState.Modified;
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public Webpage? GetWebpage(int websiteId, string path, string query)
        {
            var normalizedPath = (path ?? string.Empty).ToLowerInvariant();
            var normalizedQuery = (query ?? string.Empty).ToLowerInvariant();
            var hashInput = $"{normalizedPath}?{normalizedQuery}";
            var pathHash = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));

            return context.Webpages
                .Include(t => t.Website)
                .Where(t => t.Website.ID == websiteId && t.PathHash != null && t.PathHash == pathHash)
                .FirstOrDefault(t => t.Path == normalizedPath && t.QueryString == normalizedQuery);
        }

        public int GetVisitCount(int websiteId, string path)
        {
            if (context.VisitCounts.Any(t => t.WebsiteId == websiteId && t.Path == path))
            {
                return context.VisitCounts.First(t => t.WebsiteId == websiteId && t.Path == path).VisitCount;
            }

            return 0;
        }
    }
}
