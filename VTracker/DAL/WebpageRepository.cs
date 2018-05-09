using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using VTracker.Models;

namespace VTracker.DAL
{
    public interface IWebpageRepository : IDisposable
    {
        IEnumerable<Webpage> GetWebpages(int? websiteId);
        Webpage GetWebpageByID(int Id);
        Webpage GetWebpage(int websiteId, string path, string query);
        void InsertWebpage(Webpage w);
        void DeleteWebpage(int Id);
        void UpdateWebpage(Webpage w);
        void Save();
    }
    public class WebpageRepository : IWebpageRepository, IDisposable
    {

        private VisitTrackerContext context;

        public WebpageRepository(VisitTrackerContext context)
        {
            this.context = context;
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
        #endregion

        public IEnumerable<Webpage> GetWebpages(int? websiteId)
        {
            return context.Webpages.Where(t => t.Website.ID == websiteId || !websiteId.HasValue).ToList();
        }

        public Webpage GetWebpageByID(int Id)
        {
            return context.Webpages.Find(Id);
        }

        public void InsertWebpage(Webpage w)
        {
            context.Webpages.Add(w);
        }

        public void DeleteWebpage(int Id)
        {
            Webpage w = context.Webpages.Find(Id);
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

        public Webpage GetWebpage(int websiteId, string path, string query)
        {
            return context.Webpages.Where(t => t.Website.ID == websiteId 
            && t.Path.ToLower() == path.ToLower() && t.QueryString.ToLower() == query.ToLower()).FirstOrDefault();
        }
    }
}