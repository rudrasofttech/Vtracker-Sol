<%@ WebHandler Language="C#" Class="userhandler" %>

using System;
using System.Web;
using FileParking.Models;
using System.Web.Script.Serialization;
public class userhandler : IHttpHandler
{

    public HttpContext Context { get; set; }

    public string Action
    {
        get
        {
            if (Context.Request.QueryString["a"] != null)
            {
                return Context.Request.QueryString["a"];
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public void ProcessRequest(HttpContext context)
    {
        Context = context;
        context.Response.ContentType = "application/json";
        switch (Action)
        {
            case "email":
                context.Response.Write(GetUser());
                break;
            case "validate":
                context.Response.Write(Validate());
                break;
            default:
                context.Response.Write("");
                break;
        }

    }

    public string GetUser()
    {
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (Context.Request["email"] != null)
        {
            string email = Context.Request["email"].Trim();
            Member m = MemberManager.GetUser(email);
            Random r = new Random();
            string pass = r.Next(100000, 999999).ToString();
            if (m != null)
            {
                MemberManager.ChangePassword(m.Id, pass);
                EmailManager.SendMail(EmailManager.noreply, email, "", "", EmailManager.GetOTPEmail(pass),
                    string.Format("{0} OTP", Utility.SiteName), EmailMessageType.Communication, "OTP");
                return js.Serialize(new { email = m.Email, id = m.PublicID, isValidated = false, success = true });
            }
            else
            {
                Guid publicId = Guid.NewGuid();
                string folder = string.Format("{0}/{1}", Utility.SiteDriveFolderPath, Guid.NewGuid().ToString().ToLower());

                if (MemberManager.CreateUser(email, pass, "", "", folder, publicId))
                {
                    m = MemberManager.GetUser(email);
                    EmailManager.SendMail(EmailManager.noreply, email, "", "", EmailManager.GetSignupEmail(pass),
                        string.Format("{0} OTP", Utility.SiteName), EmailMessageType.Communication, "OTP");

                    return js.Serialize(new { email = m.Email, id = m.PublicID, isValidated = false, success = true });
                }
                else
                {
                    return js.Serialize(new { success = false, message = "Unable to register this email. Please try again." });
                }
            }
        }
        else
        {
            return js.Serialize(new { success = false, message = "No email provided." });
        }

    }

    public string Validate()
    {
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (Context.Request["email"] != null)
        {
            string email = Context.Request["email"].Trim();
            string otp = Context.Request["otp"].Trim();
            Member m = MemberManager.ValidateUser(email, otp);
            if (m != null)
            {

                string userFolderAbsolute = Context.Server.MapPath(string.Format("~/{0}", m.Folder));
                //check if user directory exist
                if (!System.IO.Directory.Exists(userFolderAbsolute))
                {
                    //create user directory
                    System.IO.Directory.CreateDirectory(userFolderAbsolute);
                }
                return js.Serialize(new { email = m.Email, id = m.PublicID, isValidated = true, token = m.AuthToken.HasValue ? m.AuthToken.Value.ToString() : "", success = true });
            }
            else
            {
                return js.Serialize(new { success = false, message = "Unable to validate the account." });
            }
        }
        else
        {
            return js.Serialize(new { success = false, message = "No Email Provided." });
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}