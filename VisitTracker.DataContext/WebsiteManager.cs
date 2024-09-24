using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisitTracker.Models;

namespace VisitTracker.DataContext
{
    public class WebsiteManager(VisitTrackerDBContext _context)
    {
        private VisitTrackerDBContext context = _context;

        public void DeleteWebsite(int websiteId)
        {
            var w = context.Websites.Find(websiteId);
            context.Websites.Remove(w);
        }

        public Website? GetWebsiteByID(int websiteId)
        {
            return context.Websites.Find(websiteId);
        }

        public IEnumerable<Website> GetWebsites(List<RecordStatus> statuses)
        {
            return context.Websites.ToList().Where(t => statuses.Contains(t.Status));
        }

        public int GetWebsiteCount(List<RecordStatus> statuses)
        {
            var query = context.Websites.ToList();
            return query.Count(t => statuses.Contains(t.Status));
        }

        public void InsertWebsite(Website w)
        {
            context.Websites.Add(w);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void UpdateWebsite(Website w)
        {
            context.Entry(w).State = EntityState.Modified;
        }
    }
}
