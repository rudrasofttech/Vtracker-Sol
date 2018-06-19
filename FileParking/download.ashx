<%@ WebHandler Language="C#" Class="download" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using FileParking;
using FileParking.Models;
using System.Web.Script.Serialization;

public class download : IHttpHandler
{
    public HttpContext Context { get; set; }
    private readonly JavaScriptSerializer js = new JavaScriptSerializer();

    public void ProcessRequest(HttpContext context)
    {
        //context.Response.ContentType = "text/plain";
        //context.Response.Write("Hello World");
        Context = context;
        Guid id = Guid.Empty;
        if (!string.IsNullOrEmpty(Context.Request["id"]))
        {
            if (!Guid.TryParse(Context.Request["id"].Trim(), out id))
            {
                context.Response.ContentType = "application/json";
                context.Response.AddHeader("Pragma", "no-cache");
                context.Response.AddHeader("Cache-Control", "private, no-cache");
                Context.Response.StatusCode = 500;
                Context.Response.TrySkipIisCustomErrors = true;
                context.Response.Write(js.Serialize(new { success = false, message = "Invalid Request" }));
                context.Response.End();
            }
        }
        else
        {
            context.Response.ContentType = "application/json";
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            context.Response.Write(js.Serialize(new { success = false, message = "Invalid Request" }));
            context.Response.End();
        }

        DownloadLink dl = null;
        Member m = null;
        using (FileParkingDataContext dc = new FileParkingDataContext())
        {
            dl = dc.DownloadLinks.SingleOrDefault(t => t.Id == id);
            if (dl != null)
            {
                m = dc.Members.SingleOrDefault(t => t.Id == dl.MemberID);
            }
        }
        if (dl != null && m != null)
        {
            string strPathName = Context.Server.MapPath(string.Format("~/{0}/{1}", m.Folder, dl.FileName));
            if (System.IO.File.Exists(strPathName))
            {
                System.IO.Stream oStream = null;
                try
                {
                    // Open the file
                    oStream =
                        new System.IO.FileStream
                            (path: strPathName,
                            mode: System.IO.FileMode.Open,
                            share: System.IO.FileShare.Read,
                            access: System.IO.FileAccess.Read);

                    // **************************************************
                    Context.Response.Buffer = false;

                    // Setting the unknown [ContentType]
                    // will display the saving dialog for the user
                    Context.Response.ContentType = "application/octet-stream";

                    // With setting the file name,
                    // in the saving dialog, user will see
                    // the [strFileName] name instead of [download]!
                    Context.Response.AddHeader("Content-Disposition", "attachment; filename=" + dl.FileName);

                    long lngFileLength = oStream.Length;

                    // Notify user (client) the total file length
                    Context.Response.AddHeader("Content-Length", lngFileLength.ToString());
                    // **************************************************

                    // Total bytes that should be read
                    long lngDataToRead = lngFileLength;

                    // Read the bytes of file
                    while (lngDataToRead > 0)
                    {
                        // The below code is just for testing! So we commented it!
                        //System.Threading.Thread.Sleep(200);

                        // Verify that the client is connected or not?
                        if (Context.Response.IsClientConnected)
                        {
                            // 8KB
                            int intBufferSize = 8 * 1024;

                            // Create buffer for reading [intBufferSize] bytes from file
                            byte[] bytBuffers =
                                new System.Byte[intBufferSize];

                            // Read the data and put it in the buffer.
                            int intTheBytesThatReallyHasBeenReadFromTheStream =
                                oStream.Read(buffer: bytBuffers, offset: 0, count: intBufferSize);

                            // Write the data from buffer to the current output stream.
                            Context.Response.OutputStream.Write
                                (buffer: bytBuffers, offset: 0,
                                count: intTheBytesThatReallyHasBeenReadFromTheStream);

                            // Flush (Send) the data to output
                            // (Don't buffer in server's RAM!)
                            Context.Response.Flush();

                            lngDataToRead =
                                lngDataToRead - intTheBytesThatReallyHasBeenReadFromTheStream;
                        }
                        else
                        {
                            // Prevent infinite loop if user disconnected!
                            lngDataToRead = -1;
                        }
                    }
                }
                catch { }
                finally
                {
                    if (oStream != null)
                    {
                        //Close the file.
                        oStream.Close();
                        oStream.Dispose();
                        oStream = null;
                    }
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
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