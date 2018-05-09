using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using VTracker.Models;

namespace VTracker.DAL
{
    public interface IWebsiteRepository : IDisposable
    {
        IEnumerable<Website> GetWebsites();
        Website GetWebsiteByID(int websiteId);
        void InsertWebsite(Website w);
        void DeleteWebsite(int websiteId);
        void UpdateWebsite(Website w);
        void Save();
    }

    public class WebsiteRepository : IWebsiteRepository, IDisposable
    {

        private VisitTrackerContext context;

        public WebsiteRepository(VisitTrackerContext context)
        {
            this.context = context;
        }

        public void DeleteWebsite(int websiteId)
        {
            Website w = context.Websites.Find(websiteId);
            context.Websites.Remove(w);
        }

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

        public Website GetWebsiteByID(int websiteId)
        {
            return context.Websites.Find(websiteId);
        }

        public IEnumerable<Website> GetWebsites()
        {
            return context.Websites.ToList();
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