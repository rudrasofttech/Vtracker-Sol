using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return context.Webpages.ToList().Count(t => statuses.Contains(t.Status));
        }

        public void DeleteWebpage(int Id)
        {
            var w = context.Webpages.First(t => t.ID ==  Id);
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
            return context.Webpages.Include(t => t.Website).Where(t => t.Website.ID == websiteId
            && t.Path == path.ToLower() && t.QueryString == query.ToLower()).FirstOrDefault();
        }
    }
}
