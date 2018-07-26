using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileParking.Models
{
    public class MemberManager
    {
        public static Member ValidateUser(string username, string password)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    Member m = (from t in dc.Members where t.Email == username && t.Password == password select t).SingleOrDefault();
                    if (m != null)
                    {
                        m.Status = (byte)GeneralStatusType.Active;
                        Guid token = Guid.NewGuid();
                        m.AuthToken = token;
                        m.TokenCreated = DateTime.UtcNow;
                        dc.SubmitChanges();
                        return m;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public static bool Update(string email, string name, string lastname, GeneralStatusType status)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    var m = (from t in dc.Members where t.Email == email select t).SingleOrDefault();
                    m.FirstName = name;
                    m.LastName = lastname;
                    m.DateModified = DateTime.UtcNow;
                    m.Status = (byte)status;
                    dc.SubmitChanges();
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public static void Delete(int id)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    var m = (from t in dc.Members where t.Id == id select t).SingleOrDefault();
                    m.Status = (byte)GeneralStatusType.Deleted;
                    dc.SubmitChanges();

                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to delete account");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
            }
        }

        public static Member GetUser(string username)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    return (from t in dc.Members where (t.Email == username) && t.Status != (byte)GeneralStatusType.Deleted select t).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public static Member GetUser(int id)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    return (from t in dc.Members where t.Id == id select t).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public static Member GetUser(Guid id)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    return (from t in dc.Members where t.PublicID == id select t).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public static Member GetUserByToken(Guid token)
        {
            Member m = CacheManager.Get<Member>(string.Format("token-{0}", token.ToString()));

            if (m == null)
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    m = dc.Members.Where(t => t.AuthToken == token).SingleOrDefault();
                    if (m != null)
                    {
                        //check if token is not older than 12 hours
                        if (m.TokenCreated.HasValue && m.TokenCreated.Value.AddHours(12) >= DateTime.UtcNow)
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

        public static List<Member> GetMemberList()
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    return (from u in dc.Members where u.Status != (byte)GeneralStatusType.Deleted orderby u.DateCreated descending select u).ToList<Member>();
                }
            }
            catch
            {
                throw;
            }
        }

        public static bool ActivateUser(int id)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    Member m = (from u in dc.Members where u.Id == id select u).SingleOrDefault();
                    m.Status = (byte)GeneralStatusType.Active;
                    dc.SubmitChanges();
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public static bool ChangePassword(int id, string password)
        {
            using (FileParkingDataContext dc = new FileParkingDataContext())
            {
                Member m = (from u in dc.Members where u.Id == id select u).SingleOrDefault();
                m.Password = password;
                dc.SubmitChanges();
                return true;
            }
        }

        public static bool CreateUser(string username, string password, string firstName, string lastName, string folder, Guid publicId)
        {
            try
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

                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    Member m = new Member();
                    m.DateCreated = DateTime.UtcNow;
                    m.Email = username;
                    m.FirstName = firstName;
                    m.Password = password;
                    m.Status = (byte)GeneralStatusType.Inactive;
                    m.LastName = lastName;
                    m.PublicID = publicId;
                    m.Folder = folder;
                    dc.Members.InsertOnSubmit(m);
                    dc.SubmitChanges();
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public static bool EmailExist(string email)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    int count = (from t in dc.Members where t.Email == email select t).Count();
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public static bool UserNameExist(string username)
        {
            try
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    int count = (from t in dc.Members where t.Email == username select t).Count();
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}