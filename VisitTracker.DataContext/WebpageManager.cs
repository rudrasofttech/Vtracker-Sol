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
            path ??= string.Empty;
            query ??= string.Empty;

            // Must match the computed SQL column:
            //   HASHBYTES('SHA2_256', LOWER([Path]) + '?' + LOWER([QueryString]))
            // SQL Server hashes nvarchar as UTF-16, which is Encoding.Unicode in .NET.
            var pathHash = SHA256.HashData(Encoding.Unicode.GetBytes(
                $"{path.ToLowerInvariant()}?{query.ToLowerInvariant()}"));

            var page = context.Webpages
                .Include(t => t.Website)
                .Where(t => t.Website.ID == websiteId && t.PathHash == pathHash)
                .OrderBy(t => t.ID)
                .FirstOrDefault(t => t.Path == path && t.QueryString == query);

            // Fallback: if the hash ever differs from SQL's (rare non-English letters where
            // .NET and SQL Server lowercase differently), find the page by its address
            // instead of creating a duplicate.
            page ??= context.Webpages
                .Include(t => t.Website)
                .Where(t => t.Website.ID == websiteId && t.Path == path && t.QueryString == query)
                .OrderBy(t => t.ID)
                .FirstOrDefault();

            return page;
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
