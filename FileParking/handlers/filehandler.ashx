<%@ WebHandler Language="C#" Class="parkedfilehandler" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FileParking.Models;
using System.Web.Script.Serialization;

public class parkedfilehandler : IHttpHandler
{
    private Member CurrentMember = null;
    private readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();
    public HttpContext Context { get; set; }

    /// <summary>
    /// Action to be taken
    /// </summary>
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

    /// <summary>
    /// Authorization Token for the Request
    /// </summary>
    public bool Token
    {
        get
        {
            if (HttpContext.Current.Request.QueryString["token"] != null)
            {
                CurrentMember = MemberManager.GetUserByToken(new Guid(HttpContext.Current.Request.QueryString["token"].Trim()));
                if (CurrentMember != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public void ProcessRequest(HttpContext context)
    {
        Context = context;
        context.Response.ContentType = "application/json";
        context.Response.AddHeader("Pragma", "no-cache");
        context.Response.AddHeader("Cache-Control", "private, no-cache");
        if (!Token)
        {
            context.Response.StatusCode = 401;
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.End();
        }
        switch (Action)
        {
            case "list":
                context.Response.Write(GetFileList());
                break;
            case "rename":
                context.Response.Write(Rename());
                break;
            case "remove":
                context.Response.Write(Remove());
                break;
            case "link":
                context.Response.Write(Download());
                break;
            default:
                context.Response.Write("");
                break;
        }

    }

    private string GetFileList()
    {
        JavaScriptSerializer js = new JavaScriptSerializer();
        DriveManager dm = new DriveManager(CurrentMember, Context.Server.MapPath("~/" + CurrentMember.Folder), "~/" + CurrentMember.Folder);
        try
        {
            return js.Serialize(new { success = true, files = dm.GetFileItemList("") });
        }
        catch (DriveDoesNotExistException)
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid User" });
        }
    }

    private string Rename()
    {
        string fileName = "";
        string newName = "";

        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!string.IsNullOrEmpty(Context.Request["f"]))
        {
            fileName = Context.Request["f"].Trim();
        }
        else
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid Request" });
        }

        if (!string.IsNullOrEmpty(Context.Request["r"]))
        {
            newName = Context.Request["r"].Trim();
        }
        else
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid Request" });
        }

        try
        {
            DriveManager dm = new DriveManager(CurrentMember, Context.Server.MapPath("~/" + CurrentMember.Folder), "~/" + CurrentMember.Folder);
            return js.Serialize(new { success = dm.RenameFile(fileName, newName) });
        }
        catch (DriveDoesNotExistException)
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid User" });
        }
        catch (DirectoryNotFoundException)
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "File not found" });
        }
    }

    private string Remove()
    {
        string fileName = "";

        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!string.IsNullOrEmpty(Context.Request["f"]))
        {
            fileName = Context.Request["f"].Trim();
        }
        else
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid Request" });
        }

        try
        {
            DriveManager dm = new DriveManager(CurrentMember, Context.Server.MapPath("~/" + CurrentMember.Folder), "~/" + CurrentMember.Folder);
            return js.Serialize(new { success = dm.DeleteFile(fileName), name = fileName });
        }
        catch (DriveDoesNotExistException)
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid User" });
        }
    }

    private string Download()
    {
        string fileName = "";

        JavaScriptSerializer js = new JavaScriptSerializer();
        if (!string.IsNullOrEmpty(Context.Request["f"]))
        {
            fileName = Context.Request["f"].Trim();
        }
        else
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid Request" });
        }

        try
        {
            DriveManager dm = new DriveManager(CurrentMember, Context.Server.MapPath("~/" + CurrentMember.Folder), "~/" + CurrentMember.Folder);
            if (dm.FileExists(fileName))
            {
                using (FileParkingDataContext dc = new FileParkingDataContext())
                {
                    DownloadLink dl = dc.DownloadLinks.SingleOrDefault(t => t.FileName.ToLower() == fileName.ToLower() && t.MemberID == CurrentMember.Id);
                    if (dl == null)
                    {
                        dl = new DownloadLink()
                        {
                            DateCreated = DateTime.UtcNow,
                            FileName = fileName,
                            Id = Guid.NewGuid(),
                            MemberID = CurrentMember.Id,
                            Password = ""
                        };
                        dc.DownloadLinks.InsertOnSubmit(dl);
                        dc.SubmitChanges();
                    }

                    return js.Serialize(new { success = false, dl = string.Format("{0}/download.ashx?id={1}", Utility.SiteURL, dl.Id) });
                }
            }
            else
            {
                Context.Response.StatusCode = 500;
                Context.Response.TrySkipIisCustomErrors = true;
                return js.Serialize(new { success = false, message = "File Not Found" });
            }
        }
        catch (DriveDoesNotExistException)
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return js.Serialize(new { success = false, message = "Invalid User" });
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