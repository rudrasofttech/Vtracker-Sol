using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace FileParking.Models
{
    public class Utility
    {
        public static string SiteName
        {
            get
            {
                return "Quick Share";
            }
        }

        public static string Prolink
        {
            get
            {
                return "#";
            }
        }

        public static string SiteURL
        {
            get
            {
                return "http://quickshare.rudrasofttech.com";
            }
        }

        public static string SiteDriveFolderPath
        {
            get
            {
                return "userdata";
            }
        }

        public static string EmailSkeleton
        {
            get
            {
                if (CacheManager.Get<string>("emailskeleton") == null)
                {
                    CacheManager.AddSliding("emailskeleton", System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/emailtemplates/emailskeleton.html")), 180);
                }
                return CacheManager.Get<string>("emailskeleton");
            }
        }

        public static string SignupEmail
        {
            get
            {
                if (CacheManager.Get<string>("signupemail") == null)
                {
                    CacheManager.AddSliding("signupemail", System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/emailtemplates/signupemail.html")), 180);
                }
                return CacheManager.Get<string>("signupemail");
            }
        }

        public static string FileShareEmail
        {
            get
            {
                if (CacheManager.Get<string>("fileshareemail") == null)
                {
                    CacheManager.AddSliding("fileshareemail", System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/emailtemplates/fileshare.html")), 180);
                }
                return CacheManager.Get<string>("fileshareemail");
            }
        }

        public static Guid ProPlanId { get { return new Guid("f04639d7-0b5d-43e2-9fb4-89f001f6c295"); } }

        public static string OTPEmail
        {
            get
            {
                if (CacheManager.Get<string>("otpemail") == null)
                {
                    CacheManager.AddSliding("otpemail", System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/emailtemplates/otpemail.html")), 180);
                }
                return CacheManager.Get<string>("otpemail");
            }
        }

        public static T Deserialize<T>(string obj)
        {
            // Create a new file stream for reading the XML file
            XmlSerializer SerializerObj = new XmlSerializer(typeof(T));
            // Load the object saved above by using the Deserialize function
            T LoadedObj = (T)SerializerObj.Deserialize(new StringReader(obj));

            return LoadedObj;
        }

        public static string Serialize<T>(T obj)
        {
            // Create a new XmlSerializer instance with the type of the test class
            XmlSerializer SerializerObj = new XmlSerializer(typeof(T));

            // Create a new file stream to write the serialized object to a file
            TextWriter WriteFileStream = new StringWriter();
            SerializerObj.Serialize(WriteFileStream, obj);

            // Cleanup
            return WriteFileStream.ToString();
        }

        
        
        public static string ImageFormat()
        {
            return ".bmp,.dds,.dng,.gif,.jpg,.png,.psd,.psd,.pspimage,.tga,.thm,.tif,.yuv,.ai,.eps,.ps,.svg";
        }

        public static string VideoFormat()
        {
            return ".3g2,.3gp,.asf,.asx,.flv,.mov,.mp4,.mpg,.rm,.srt,.swf,.vob,.wmv";
        }

        public static string TextFormat()
        {
            return ".doc,.docx,.log,.msg,.odt,.pages,.rtf,.tex,.txt,.wpd,.wps";
        }

        public static string CompresssedFormat()
        {
            return ".7z,.cbr,.deb,.gz,.pkg,.rar,.rpm,.sit,.sitx,.tar.gz,.zip,.zipx";
        }

        public static string UrlFriendlyTitle(string title)
        {
            return HttpContext.Current.Server.UrlEncode(title.Trim().Replace(" ", "-").Replace(":", "-"));
        }

        public static string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static string Slugify(string phrase)
        {
            string str = RemoveAccent(phrase).ToLower();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9/\s-]", ""); // Remove all non valid chars          
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space  
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-"); // //Replace spaces by dashes
            return str;
        }

        #region Validation Functions
        public static bool ValidateEmail(string email)
        {
            string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        public static bool ValidateRequired(string input)
        {
            if (input.Trim() == string.Empty)
            {
                return false;
            }
            else { return true; }
        }
        #endregion
    }
}