using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VTracker.DAL;

namespace VTracker.Models
{
    public class MemberManager
    {
        //private IWebsiteRepository websiteRepository;
        //private IVisitRepository visitRepository;
        //private IWebpageRepository webpageRepository;
        public IMemberRepository memberRepository;
        //private IMemberWebsiteRepository memberWebsiteRepository;
        private VisitTrackerContext context;

        public MemberManager()
        {
            context = new VisitTrackerContext();
            this.memberRepository = new MemberRepository(context);
        }

        public Member ValidateUser(string username, string password)
        {
            try
            {
                Member m = this.memberRepository.GetMember(username, password);
                if (m != null)
                {
                    m.Status = RecordStatus.Active;
                    Guid token = Guid.NewGuid();
                    m.AuthToken = token;
                    m.TokenCreated = DateTime.UtcNow;
                    this.memberRepository.UpdateMember(m);
                    this.memberRepository.Save();
                    return m;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Update(string email, string name, string lastname, RecordStatus status)
        {
            try
            {

                var m = this.memberRepository.GetMemberByEmail(email);
                m.FirstName = name;
                m.LastName = lastname;
                m.DateModified = DateTime.UtcNow;
                m.Status = status;
                this.memberRepository.UpdateMember(m);
                memberRepository.Save();
                return true;

            }
            catch
            {
                throw;
            }
        }

        public void Delete(int id)
        {
            try
            {
                var m = memberRepository.GetMemberByID(id);
                m.Status = RecordStatus.Deleted;
                this.memberRepository.UpdateMember(m);
                memberRepository.Save();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to delete account");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
            }
        }

        public Member GetUser(string username)
        {

            var m = memberRepository.GetMemberByEmail(username);
            if (m.Status != RecordStatus.Deleted)
            {
                return m;
            }
            else
            {
                return null;
            }


        }

        public Member GetUser(int id)
        {

            return memberRepository.GetMemberByID(id);

        }

        public Member GetUser(Guid id)
        {
            return memberRepository.GetMemberByPublicID(id);
        }

        public Member GetUserByToken(Guid token)
        {
            Member m = CacheManager.Get<Member>(string.Format("token-{0}", token.ToString()));

            if (m == null)
            {

                m = memberRepository.GetMemberByAuthToken(token);
                if (m != null)
                {
                    //check if token is not older than 48 hours
                    if (m.TokenCreated.HasValue && m.TokenCreated.Value.AddHours(5) >= DateTime.UtcNow)
                    {
                        CacheManager.AddSliding(string.Format("token-{0}", token.ToString()), m, 240);
                        return m;
                    }
                    else { return null; }
                }
                else
                {
                    return null;
                }

            }
            else
            {
                if (m.TokenCreated.HasValue && m.TokenCreated.Value.AddHours(12) >= DateTime.UtcNow)
                {
                    return m;
                }
                else
                {
                    CacheManager.Remove(string.Format("token-{0}", token.ToString()));
                    return null;
                }
            }
        }

        public List<Member> GetMemberList()
        {
            return memberRepository.GetMembers().ToList<Member>();
        }

        public bool ActivateUser(int id)
        {

            Member m = memberRepository.GetMemberByID(id);
            m.Status = RecordStatus.Active;
            memberRepository.UpdateMember(m);
            memberRepository.Save();
            return true;

        }

        public bool ChangePassword(int id, string password)
        {

            Member m = memberRepository.GetMemberByID(id);
            m.Password = password;
            memberRepository.UpdateMember(m);
            memberRepository.Save();
            return true;

        }

        public bool CreateUser(string username, string password, string firstName, string lastName, string folder, Guid publicId)
        {

            if (username.Trim() == string.Empty)
            {
                return false;
            }
            if (password.Trim() == string.Empty)
            {
                return false;
            }
            if (EmailExist(username))
            {
                return false;
            }


            Member m = new Member();
            m.DateCreated = DateTime.UtcNow;
            m.Email = username;
            m.FirstName = firstName;
            m.Password = password;
            m.Status = RecordStatus.Inactive;
            m.LastName = lastName;
            m.PublicId = publicId;

            memberRepository.InsertMember(m);
            memberRepository.Save();
            return true;

        }

        public bool EmailExist(string email)
        {

            Member m = memberRepository.GetMemberByEmail(email);
            if (m != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}