using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using VTracker.Models;

namespace VTracker.DAL
{
    public interface IMemberRepository : IDisposable
    {
        IEnumerable<Member> GetMembers();
        Member GetMemberByID(int id);
        Member GetMemberByPublicID(Guid id);
        Member GetMemberByEmail(string email);
        Member GetMember(string email, string password);
        void InsertMember(Member m);
        void DeleteMember(int id);
        void UpdateMember(Member m);
        void Save();
    }

    public class MemberRepository : IMemberRepository, IDisposable
    {
        private VisitTrackerContext context;

        public MemberRepository(VisitTrackerContext context)
        {
            this.context = context;
        }

        public void DeleteMember(int id)
        {
            Member m = context.Members.Find(id);
            context.Members.Remove(m);
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

        public Member GetMember(string email, string password)
        {
            return context.Members.Where(t => t.Email == email && t.Password == password).FirstOrDefault();
            
        }

        public Member GetMemberByEmail(string email)
        {
            return context.Members.Where(t => t.Email == email ).FirstOrDefault();
        }

        public Member GetMemberByID(int id)
        {
            return context.Members.Where(t => t.ID == id).FirstOrDefault();
        }

        public Member GetMemberByPublicID(Guid id)
        {
            return context.Members.Where(t => t.PublicId == id).FirstOrDefault();
        }

        public IEnumerable<Member> GetMembers()
        {
            return context.Members;
        }

        public void InsertMember(Member m)
        {
            context.Members.Add(m);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void UpdateMember(Member m)
        {
            context.Entry(m).State = EntityState.Modified;
        }
    }
}