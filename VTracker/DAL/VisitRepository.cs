using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using VTracker.Models;

namespace VTracker.DAL
{
    public interface IVisitRepository : IDisposable
    {
        IEnumerable<Visit> GetVisits();
        IEnumerable<Visit> GetVisitsByWebpage(int webpageId);
        Visit GetVisitByID(int id);
        Visit GetVisitByCC(Guid cc);
        void InsertVisit(Visit w);
        void DeleteVisit(int id);
        void UpdateVisit(Visit w);
        void Save();

        IEnumerable<VisitPage> GetVisitPages(int visitId);
        VisitPage GetVisitPageByID(int id);
        VisitPage GetVisitPageByVisitAndWebpage(int visitId, int webpageId);
        void InsertVisitPage(VisitPage w);
        void DeleteVisitPage(int id);
        void UpdateVisitPage(VisitPage w);
        VisitPage GetLastVisitedPageofVisit(int visitId);

        IEnumerable<VisitActivity> GetVisitPageActivities(int visitId);
        VisitActivity GetVisitPageActivityByID(int id);
        IEnumerable<VisitActivity> GetVisitPageActivityByVisitAndWebpage(int visitId, int webpageId);
        void InsertVisitPageActivity(VisitActivity w);
        void DeleteVisitPageActivity(int id);
        void UpdateVisitPageActivity(VisitActivity w);
        
    }

    public class VisitRepository : IVisitRepository, IDisposable
    {
        private VisitTrackerContext context;

        public VisitRepository(VisitTrackerContext context)
        {
            this.context = context;
        }

        public void DeleteVisit(int id)
        {
            Visit v = context.Visits.Find(id);
            context.Visits.Remove(v);
        }

        public Visit GetVisitByID(int id)
        {
            return context.Visits.Find(id);
        }

        public IEnumerable<Visit> GetVisits()
        {
            return context.Visits.ToList();
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
            return context.VisitPages.Where(t => t.visit.ID == visitId).OrderBy(t => t.ID).ToList();
        }

        public VisitPage GetVisitPageByID(int id)
        {
            return context.VisitPages.Find(id);
        }

        public void InsertVisitPage(VisitPage w)
        {
            context.VisitPages.Add(w);
        }

        public void DeleteVisitPage(int id)
        {
            VisitPage v = context.VisitPages.Find(id);
            context.VisitPages.Remove(v);
        }

        public void UpdateVisitPage(VisitPage w)
        {
            context.Entry(w).State = EntityState.Modified;
        }

        public Visit GetVisitByCC(Guid cc)
        {
            return context.Visits.Where(t => t.ClientCookie == cc).FirstOrDefault();
        }

        public VisitPage GetVisitPageByVisitAndWebpage(int visitId, int webpageId)
        {
            return context.VisitPages.Where(t => t.visit.ID == visitId && t.webpage.ID == webpageId).FirstOrDefault();
        }

        public VisitPage GetLastVisitedPageofVisit(int visitId)
        {
            return context.VisitPages.Where(t => t.visit.ID == visitId).OrderByDescending(t => t.ID).FirstOrDefault();
        }

        public IEnumerable<Visit> GetVisitsByWebpage(int webpageId)
        {
            return context.VisitPages.Where(t => t.webpage.ID == webpageId).OrderBy(t => t.DateCreated).Select(t => t.visit).Distinct();
        }
        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<VisitActivity> GetVisitPageActivities(int visitId)
        {
            return context.VisitPageActivities.Where(t => t.visit.ID == visitId);
        }

        public VisitActivity GetVisitPageActivityByID(int id)
        {
            return context.VisitPageActivities.Where(t => t.ID == id).FirstOrDefault();
        }

        public IEnumerable<VisitActivity> GetVisitPageActivityByVisitAndWebpage(int visitId, int webpageId)
        {
            return context.VisitPageActivities.Where(t => t.visit.ID == visitId && t.visitpage.webpage.ID == webpageId);
        }

        public void InsertVisitPageActivity(VisitActivity w)
        {
            context.VisitPageActivities.Add(w);
        }

        public void DeleteVisitPageActivity(int id)
        {
            VisitActivity v = context.VisitPageActivities.Find(id);
            context.VisitPageActivities.Remove(v);
        }

        public void UpdateVisitPageActivity(VisitActivity w)
        {
            context.Entry(w).State = EntityState.Modified;
        }


        #endregion

    }
}