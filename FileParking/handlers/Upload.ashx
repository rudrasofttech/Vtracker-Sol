﻿
<%@ WebHandler Language="C#" Class="Upload" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using FileParking;
using FileParking.Models;

public class Upload : IHttpHandler
{

    #region Fields
    private Member CurrentMember = null;
    private readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();
    private DriveManager DM;
    #endregion

    #region Properties

    public Member CurrentUser { get; set; }

    public bool IsReusable { get { return false; } }

    public string StorageFolder
    {
        get
        {

            if (Token)
            {
                return CurrentMember.Folder;
            }
            return "";
            //return HttpContext.Current.Request.QueryString["storageFolder"];
        }
    }

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

    public long MaxChunkSize
    {
        get
        {
            return long.Parse(HttpContext.Current.Request.QueryString["maxChunkSize"]);
        }
    }

    public bool Resume
    {
        get
        {
            return bool.Parse(HttpContext.Current.Request.QueryString["resume"]);
        }
    }

    #endregion

    #region Methods

    public void ProcessRequest(HttpContext context)
    {
        context.Response.AddHeader("Pragma", "no-cache");
        context.Response.AddHeader("Cache-Control", "private, no-cache");
        if (!Token)
        {
            context.Response.StatusCode = 401;
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.End();
        }


        DM = new DriveManager(new Member(), HttpContext.Current.Server.MapPath(string.Format("~/{0}", CurrentMember.Folder)), string.Format("{0}/{1}", Utility.SiteURL, CurrentMember.Folder));

        // Cross-site chunked uploads
        context.Response.AddHeader("Access-Control-Allow-Headers", "X-File-Name,X-File-Type,X-File-Size");
        DoWork(context);
    }

    #endregion

    #region Helpers

    // Handle request based on method
    private void DoWork(HttpContext context)
    {
        switch (context.Request.HttpMethod)
        {
            case "HEAD":
            case "GET":
                if (GivenFilename(context))
                {
                    if (Resume)
                    {
                        SendFileInfo(context);
                    }
                    else
                    {
                        DeliverFile(context);
                    }
                }
                else
                {
                    ListCurrentFiles(context);
                }
                break;
            case "POST":
            case "PUT":
                if (DM.RemainingFileLimit() > 0)
                {
                    UploadFile(context);
                }
                break;
            case "DELETE":
                DeleteFile(context);
                break;
            case "OPTIONS":
                ReturnOptions(context);
                break;
            default:
                context.Response.ClearHeaders();
                context.Response.StatusCode = 405;
                break;
        }
    }

    private static bool GivenFilename(HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Request["f"]);
    }

    private void ListCurrentFiles(HttpContext context)
    {

        //FileStatus[] statuses =
        //    (from file in new DirectoryInfo(context.Server.MapPath("~/" + StorageFolder)).GetFiles("*", SearchOption.TopDirectoryOnly)
        //     where !file.Attributes.HasFlag(FileAttributes.Hidden)
        //     select new FileStatus(file, StorageFolder)).ToArray();
        //string jsonObj = _javaScriptSerializer.Serialize(statuses);
        context.Response.AddHeader("Content-Disposition", "inline; filename=\"files.json\"");
        context.Response.ContentType = "application/json";
        context.Response.Write("");
        //context.Response.Write(jsonObj);
    }

    private void SendFileInfo(HttpContext context)
    {
        string filename = context.Request["f"];
        string filePath = Path.Combine(StorageFolder, filename);
        if (!File.Exists(context.Server.MapPath("~\\" + filePath)))
        {
            FileStatus status = File.Exists(filePath)
                                ? new FileStatus(new FileInfo(filePath), StorageFolder)
                                : new FileStatus(filename, 0, StorageFolder);
            string jsonObj = _javaScriptSerializer.Serialize(status);
            context.Response.AddHeader("Content-Disposition", "inline; filename=\"files.json\"");
            context.Response.ContentType = "application/json";
            context.Response.Write(jsonObj);
        }
        else
        {
            FileStatus status = new FileStatus(filename, 0, StorageFolder);
            status.error = "File already exist";
            string jsonObj = _javaScriptSerializer.Serialize(status);
            context.Response.AddHeader("Content-Disposition", "inline; filename=\"files.json\"");
            context.Response.ContentType = "application/json";
            context.Response.Write(jsonObj);
        }
    }

    private void DeliverFile(HttpContext context)
    {
        string filename = context.Request["f"];
        string filePath = Path.Combine(StorageFolder, filename);

        if (File.Exists(filePath))
        {
            context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
            context.Response.ContentType = "application/octet-stream";
            context.Response.ClearContent();
            context.Response.WriteFile(filePath);
        }
        else
        {
            context.Response.StatusCode = 404;
        }
    }

    // Delete file from the server
    private void DeleteFile(HttpContext context)
    {
        //var filePath = Path.Combine(StorageFolder, context.Request["f"]);
        //if (File.Exists(filePath))
        //{
        //    File.Delete(filePath);
        //}
    }

    // Upload file to the server
    private void UploadFile(HttpContext context)
    {
        List<FileStatus> statuses = new List<FileStatus>();

        try
        {
            NameValueCollection headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                UploadWholeFile(context, statuses);
            }
            else
            {
                UploadPartialFile(headers["X-File-Name"], context, statuses);
            }
        }
        finally
        {
            WriteJsonIframeSafe(context, statuses);
        }
    }

    // Upload partial file
    private void UploadPartialFile(string fileName, HttpContext context, List<FileStatus> statuses)
    {
        NameValueCollection headers = context.Request.Headers;

        //
        // Retrieve chunks information from the request
        // 
        int chunkSize = int.Parse(headers["Content-Length"]);
        //int chunksNumber = int.Parse(headers["X-Chunks-Number"] ?? "0"); // not needed now
        int chunkIndex = int.Parse(headers["X-Chunk-Index"] ?? "0");

        string fullName = Path.Combine(context.Server.MapPath("~/" + StorageFolder), Path.GetFileName(fileName));
        long fileLength;// in Bytes!
        if (Resume)
        {
            long previousLength = GetFileSize(fullName);
            fileLength = previousLength + chunkSize;
        }
        else
        {
            fileLength = chunkIndex * MaxChunkSize + chunkSize; // If not resuming, no need to use GetFileSize
        }

        FileStatus status = new FileStatus(fileName, fileLength, StorageFolder);

        try
        {
            const int bufferSize = 1024;
            Stream inputStream = context.Request.InputStream;

            // FileShare.Delete for fileuploadfail event when resuming is disabled
            using (FileStream fileStream =
                new FileStream(fullName, FileMode.Append, FileAccess.Write, FileShare.Delete))
            {
                byte[] buffer = new byte[bufferSize];

                int l;
                while ((l = inputStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    fileStream.Write(buffer, 0, l);
                }

                fileStream.Flush();
            }
        }
        catch (Exception e)
        {
            status.error = e.Message;
        }

        statuses.Add(status);
    }

    // Get file size
    private long GetFileSize(string fullName)
    {
        long fileSize = File.Exists(fullName)
                            ? new FileInfo(fullName).Length
                            : 0;
        return fileSize;
    }

    //// Upload entire file
    private void UploadWholeFile(HttpContext context, List<FileStatus> statuses)
    {

        for (int i = 0; i < context.Request.Files.Count; i++)
        {
            HttpPostedFile file = context.Request.Files[i];
            string fileName = Path.GetFileName(file.FileName);
            FileStatus status = new FileStatus(fileName, file.ContentLength, StorageFolder);

            try
            {
                DM.UploadFile(StorageFolder, file);
            }
            catch (Exception e)
            {
                status.error = e.Message;
            }

            statuses.Add(status);
        }
    }

    private static void ReturnOptions(HttpContext context)
    {
        context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
        context.Response.StatusCode = 200;
    }

    private void WriteJsonIframeSafe(HttpContext context, List<FileStatus> statuses)
    {
        context.Response.AddHeader("Vary", "Accept");
        try
        {
            context.Response.ContentType = context.Request["HTTP_ACCEPT"].Contains("application/json") ? "application/json" : "text/plain";
        }
        catch
        {
            context.Response.ContentType = "text/plain";
        }

        string status = _javaScriptSerializer.Serialize(statuses.ToArray());
        context.Response.Write(status);
    }

    #endregion
}
