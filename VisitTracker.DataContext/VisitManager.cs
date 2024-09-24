using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisitTracker.Models;

namespace VisitTracker.DataContext
{
    public class VisitManager(VisitTrackerDBContext _context)
    {
        private VisitTrackerDBContext context = _context;

        public void DeleteVisit(int id)
        {
            Visit v = context.Visits.First(t => t.ID == id);
            context.Visits.Remove(v);
            context.SaveChanges();
        }

        public Visit GetVisitByID(int id)
        {
            return context.Visits.First(t => t.ID == id);
        }

        public IEnumerable<Visit> GetVisits()
        {
            return context.Visits.ToList();
        }

        public int GetVisitCount()
        {
            return context.Visits.Count();
        }

        public int GetVisitCount(int websiteId)
        {
            return context.Visits.Include(t => t.Website).Count(i => i.Website.ID == websiteId);
        }

        public IEnumerable<Visit> GetVisits(int websiteId)
        {
            return context.Visits.Include(t => t.Website).Where(t => t.Website.ID == websiteId).OrderByDescending(t => t.DateCreated).ToList();
        }

        public List<Visit> GetVisits(int websiteId, DateTime? start, DateTime? end, int page = 0, int pageSize = 50)
        {
            var query = context.Visits.Include(t => t.Website).Include(t => t.VisitPages).Where(t => t.Website.ID == websiteId);
            if(start.HasValue)
                query =query.Where(t => t.DateCreated >= start.Value);
            if(end.HasValue)
                query =query.Where(t => t.DateCreated <= end.Value);

            query = query.OrderByDescending(t => t.DateCreated);

            return [.. query.Skip(page * pageSize).Take(pageSize)];
        }

        public void InsertVisit(Visit w)
        {
            context.Visits.Add(w);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void UpdateVisit(Visit w)
        {
            context.Entry(w).State = EntityState.Modified;
        }

        public IEnumerable<VisitPage> GetVisitPages(int visitId)
        {
            return context.VisitPages.Include(t => t.visit).Where(t => t.visit.ID == visitId).OrderBy(t => t.ID).ToList();
        }

        public VisitPage? GetVisitPageByID(int id)
        {
            return context.VisitPages.Find(id);
        }

        public void InsertVisitPage(VisitPage w)
        {
            context.VisitPages.Add(w);
        }

        public void DeleteVisitPage(int id)
        {
            var v = context.VisitPages.Find(id);
            context.VisitPages.Remove(v);
        }

        public void UpdateVisitPage(VisitPage w)
        {
            context.Entry(w).State = EntityState.Modified;
        }

        public Visit? GetVisitByCC(Guid cc)
        {
            return context.Visits.Include(t => t.Website).FirstOrDefault(t => t.ClientCookie == cc);
        }

        public VisitPage? GetVisitPageByVisitAndWebpage(int visitId, int webpageId)
        {
            return context.VisitPages.Include(t => t.visit).Include(t => t.webpage).Where(t => t.visit.ID == visitId && t.webpage.ID == webpageId).FirstOrDefault();
        }

        public VisitPage? GetLastVisitedPageofVisit(int visitId)
        {
            return context.VisitPages.Include(t => t.visit).Include(t => t.webpage).Where(t => t.visit.ID == visitId).OrderByDescending(t => t.ID).FirstOrDefault();
        }

        public IEnumerable<Visit> GetVisitsByWebpage(int webpageId, DateTime? start, DateTime? end)
        {
            return context.VisitPages.Include(t => t.visit).Include(t => t.webpage).Where(t => t.webpage.ID == webpageId
            && t.visit.DateCreated >= start && t.visit.DateCreated <= end).OrderBy(t => t.DateCreated).Select(t => t.visit).Distinct();
        }

        public IEnumerable<Visit> GetVisitsByWebpage(int webpageId)
        {
            return context.VisitPages.Include(t => t.webpage).Where(t => t.webpage.ID == webpageId).OrderBy(t => t.DateCreated).Select(t => t.visit).Distinct();
        }
        


        public IEnumerable<VisitActivity> GetVisitPageActivities(int visitId)
        {
            return context.VisitActivities.Include(t => t.visit).Where(t => t.visit.ID == visitId);
        }

        public IEnumerable<VisitActivity> GetVisitPageActivities(List<int> visitIds, int? webpageId)
        {
            if (webpageId.HasValue)
            {
                var query = context.VisitActivities.Include(t => t.visit).Include(t => t.visitpage)
                    .Include(t => t.visitpage.webpage).Where(t => t.visitpage.webpage.ID == webpageId);

                var list = query.ToList();
                return list.Where(t => visitIds.Contains(t.visit.ID));
            }
            else
            {
                return context.VisitActivities.Include(t => t.visit).Where(t => visitIds.Contains(t.visit.ID)).ToList();

            }
        }

        public VisitActivity? GetVisitPageActivityByID(int id)
        {
            return context.VisitActivities.FirstOrDefault(t => t.ID == id);
        }

        public IEnumerable<VisitActivity> GetVisitPageActivityByVisitAndWebpage(int visitId, int webpageId)
        {
            return context.VisitActivities.Include(t => t.visit).Include(t => t.visitpage).Include(t => t.visitpage.webpage).Where(t => t.visit.ID == visitId && t.visitpage.webpage.ID == webpageId);
        }

        public void InsertVisitPageActivity(VisitActivity w)
        {
            context.VisitActivities.Add(w);
            
        }

        public void DeleteVisitPageActivity(int id)
        {
            VisitActivity v = context.VisitActivities.First(t => t.ID == id);
            context.VisitActivities.Remove(v);
        }

        public void UpdateVisitPageActivity(VisitActivity w)
        {
            context.Entry(w).State = EntityState.Modified;
        }

        public IEnumerable<Visit> GetVisits(int websiteId, DateTime start, DateTime end)
        {
            return context.Visits.Include(t => t.Website).Where(t => t.Website.ID == websiteId && t.DateCreated >= start && t.DateCreated <= end).OrderByDescending(t => t.DateCreated);
        }

        public int GetVisitCount(int websiteId, DateTime start, DateTime end) {
            return context.Visits.Count(t => t.Website.ID == websiteId && t.DateCreated >= start && t.DateCreated <= end);
        }

        public List<VisitPage> GetVisitPagesByWebsiteIdAndDateRange(int websiteId, DateTime start, DateTime end)
        {
            return context.VisitPages.Include(t => t.visit).Include(t => t.visit.Website).Where(t => t.visit.Website.ID == websiteId
            && t.DateCreated >= start
            && t.DateCreated <= end).ToList();
        }



        public List<Tuple<Visit, Webpage>> GetVisitAndWebpageByWebsite(int websiteId, DateTime? start, DateTime? end)
        {
            //var list = context.VisitPages.Include(t => t.visit).Include(t => t.webpage).Include(t => t.visit.Website).Where(t => t.visit.Website.ID == websiteId
            //&& (start.HasValue && (t.DateCreated.Year >= start.Value.Year && t.DateCreated.Month >= start.Value.Month && t.DateCreated.Day >= start.Value.Day))
            //&& (end.HasValue && (t.DateCreated.Year <= end.Value.Year && t.DateCreated.Month <= end.Value.Month && t.DateCreated.Day <= end.Value.Day)))
            //    .Select(t => new { V = t.visit, WP = t.webpage }).Distinct().ToList();

            var query = context.VisitPages.Include(t => t.visit).Include(t => t.webpage).Include(t => t.visit.Website).Where(t => t.visit.Website.ID == websiteId);
            if (start.HasValue)
                query = query.Where(t => t.DateCreated >= start.Value);
            if (end.HasValue)
                query = query.Where(t => t.DateCreated <= end.Value);

            var list = query.Select(t => new { V = t.visit, WP = t.webpage }).Distinct();

            var result = new List<Tuple<Visit, Webpage>>();
            foreach (var i in list)
            {
                result.Add(new Tuple<Visit, Webpage>(i.V, i.WP));
            }

            return result;
        }

        public IEnumerable<VisitActivity> GetAcitivites()
        {
            return context.VisitActivities.ToList();
        }

        public int GetActivityCount()
        {
            return context.VisitActivities.Count();
        }

        public int GetActivityCount(int websiteId)
        {
            return context.VisitActivities.Include(t => t.visit).Include(t => t.visit.Website).Count(i => i.visit.Website.ID == websiteId);
        }
    }
}
