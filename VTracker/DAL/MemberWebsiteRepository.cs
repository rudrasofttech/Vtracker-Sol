using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using VTracker.Models;

namespace VTracker.DAL
{
    public interface IMemberWebsiteRepository : IDisposable
    {
        IEnumerable<MemberWebsiteRelation> GetWebsiteRelations(int websiteid);
        IEnumerable<MemberWebsiteRelation> GetMemberWebsites(int memberId);

        void InsertMemberWebsiteRelation(MemberWebsiteRelation m);
        void DeleteMemberWebsiteRelation(int memberid, int websiteid);
        void UpdateMemberWebsiteRelation(MemberWebsiteRelation m);
        void Save();
    }

    public class MemberWebsiteRepository : IMemberWebsiteRepository, IDisposable
    {

        private VisitTrackerContext context;
        public MemberWebsiteRepository(VisitTrackerContext context)
        {
            this.context = context;
        }
        public void DeleteMemberWebsiteRelation(int memberid, int websiteid)
        {
            MemberWebsiteRelation m = context.MemberWebsiteRelations.SingleOrDefault(t => t.Member.ID == memberid && t.Website.ID == websiteid);
            if (m != null)
            {
                context.MemberWebsiteRelations.Remove(m);
            }
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

        public IEnumerable<MemberWebsiteRelation> GetMemberWebsites(int memberId)
        {
            return context.MemberWebsiteRelations.Where(t => t.Member.ID == memberId);
        }

        public IEnumerable<MemberWebsiteRelation> GetWebsiteRelations(int websiteid)
        {
            return context.MemberWebsiteRelations.Where(t => t.Website.ID == websiteid);
        }

        public void InsertMemberWebsiteRelation(MemberWebsiteRelation m)
        {
            context.MemberWebsiteRelations.Add(m);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void UpdateMemberWebsiteRelation(MemberWebsiteRelation m)
        {
            context.Entry(m).State = EntityState.Modified;
        }
    }
}