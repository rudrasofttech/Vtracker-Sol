using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileParking;
using System.Net.Mail;

namespace FileParking.Models
{
    public enum EmailMessageType
    {
        Activation = 1,
        Unsubscribe = 2,
        Newsletter = 3,
        ChangePassword = 4,
        Reminder = 5,
        Communication = 6,
        Notification = 7
    }

    public class EmailManager
    {
        public const string noreply = "noreply@fileparking.com";

        public EmailManager()
        {
        }

        public static string GetSignupEmail(string password) {
            string msg = Utility.SignupEmail.Replace("[otp]", password);
            //string emailtemplate = Utility.EmailSkeleton.Replace("[message]", msg);
            //emailtemplate = emailtemplate.Replace("[noreply]", noreply).Replace("[sitename]", Utility.SiteName).Replace("[prolink]", Utility.Prolink);
            return msg;
        }

        public static string GetOTPEmail(string password)
        {
            string msg = Utility.OTPEmail.Replace("[otp]", password);
            //string emailtemplate = Utility.EmailSkeleton.Replace("[message]", msg);
            //emailtemplate = emailtemplate.Replace("[noreply]", noreply).Replace("[sitename]", Utility.SiteName).Replace("[prolink]", Utility.Prolink);
            return msg;
        }

        public static bool SendMail(String fromAddress, String toAddress, String senderName, String recieverName, String body, String subject, EmailMessageType messageType, string emailGroup)
        {
            return SendMail(fromAddress, toAddress, senderName, recieverName, body, subject, string.Empty, messageType, emailGroup);
        }

        public static bool SendMail(EmailMessage em)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(new MailAddress(em.ToAddress, em.ToName));
                if (em.CCAdress.Trim() != string.Empty)
                {
                    mail.CC.Add(em.CCAdress);
                }
                mail.From = new MailAddress(em.FromAddress, em.FromName);
                mail.Subject = em.Subject;
                mail.Body = em.Message;
                mail.IsBodyHtml = true;
                System.Net.Mail.SmtpClient client = new SmtpClient();
                client.Send(mail);
                try
                {
                    em.LastAttempt = DateTime.Now;
                    em.IsSent = true;
                    em.SentDate = DateTime.Now;
                    UpdateMessage(em);
                }
                catch
                {
                }
                return true;

            }
            catch (Exception ex)
            {
                try
                {
                    em.LastAttempt = DateTime.Now;
                    em.IsSent = false;
                    UpdateMessage(em);
                }
                catch
                {
                }
                HttpContext.Current.Trace.Write("Unable to send email.");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
                return false;
            }
        }

        public static bool SendMail(String fromAddress, String toAddress,
            String senderName, String recieverName, String body, String subject,
            string ccaddresses, EmailMessageType messageType, string emailGroup)
        {
            try
            {
                EmailMessage em = new EmailMessage();
                em.ID = Guid.NewGuid();

                string emessage = Utility.EmailSkeleton;
                emessage = emessage.Replace("[root]", Utility.SiteURL);
                //emessage = emessage.Replace("[newsletteremail]", Utility.NewsletterEmail);
                emessage = emessage.Replace("[message]", body);
                emessage = emessage.Replace("[id]", em.ID.ToString());
                emessage = emessage.Replace("[toaddress]", toAddress);
                emessage = emessage.Replace("[sitename]", Utility.SiteName);
                //emessage = emessage.Replace("[emailsignature]", Utility.GetSiteSetting("emailsignature"));
                em.Message = emessage;

                em = AddMessage(em.ID, toAddress, fromAddress, subject, emessage, messageType, emailGroup,ccaddresses, recieverName, senderName);

                if (em != null)
                {
                    return SendMail(em);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to send email.");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
                return false;
            }
        }

        public static bool UpdateMessage(EmailMessage em)
        {
            try
            {
                using (FileParkingDataContext db = new FileParkingDataContext())
                {
                    EmailMessage item = (from u in db.EmailMessages where u.ID == em.ID select u).SingleOrDefault();
                    item.CreateDate = em.CreateDate;
                    item.EmailGroup = em.EmailGroup;
                    item.EmailType = em.EmailType;
                    item.FromAddress = em.FromAddress;
                    item.IsRead = em.IsRead;
                    item.IsSent = em.IsSent;
                    item.Message = em.Message;
                    item.SentDate = em.SentDate;
                    item.Subject = em.Subject;
                    item.ToAddress = em.ToAddress;
                    item.ReadDate = em.ReadDate;
                    db.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to save EmailMessage object to database.");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
                return false;
            }
        }

        public static EmailMessage GetMessage(Guid id)
        {
            try
            {
                using (FileParkingDataContext db = new FileParkingDataContext())
                {
                    EmailMessage em = (from u in db.EmailMessages where u.ID == id select u).SingleOrDefault();
                    return em;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to get email message database.");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
                return null;
            }
        }

        public static EmailMessage GetUnsentMessage()
        {
            try
            {
                using (FileParkingDataContext db = new FileParkingDataContext())
                {
                    EmailMessage em = (from u in db.EmailMessages where u.IsSent == false orderby u.LastAttempt ascending select u).FirstOrDefault();
                    return em;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to get email message database.");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Saves email message details to database
        /// </summary>
        /// <param name="toaddress"></param>
        /// <param name="fromaddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="messagetype"></param>
        /// <returns></returns>
        public static EmailMessage AddMessage(Guid id, 
            string toaddress, 
            string fromaddress, 
            string subject, 
            string body, 
            EmailMessageType messagetype, 
            string emailGroup,
            string ccaddress,
            string toname,
            string fromname)
        {
            try
            {
                using (FileParkingDataContext db = new FileParkingDataContext())
                {
                    EmailMessage em = new EmailMessage()
                    {
                        ID = id,
                        Message = body,
                        FromAddress = fromaddress,
                        EmailType = (byte)messagetype,
                        Subject = subject,
                        ToAddress = toaddress,
                        SentDate = DateTime.Now,
                        CreateDate = DateTime.Now,
                        IsRead = false,
                        IsSent = false,
                        EmailGroup = emailGroup,
                        CCAdress = ccaddress,
                        ToName = toname,
                        FromName = fromname,
                        LastAttempt = DateTime.Now
                    };
                    db.EmailMessages.InsertOnSubmit(em);
                    db.SubmitChanges();
                    return em;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Write("Unable to save EmailMessage object to database.");
                HttpContext.Current.Trace.Write(ex.Message);
                HttpContext.Current.Trace.Write(ex.StackTrace);
                return null;
            }
        }
    }
}