using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using VTracker.Models;

namespace VTracker
{
    
    public class HeatMapGenerator
    {

        private string _hmpath = string.Empty;
       
        public string HeatMapPath
        {
            get
            {
                return _hmpath;
            }
        }
        /// <summary>
        /// Webpage Image Avialable
        /// </summary>
        public bool wia
        {
            get
            {
                if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(string.Format("~/wpi/{0}-{1}.jpeg", ws.ID, wp.ID))))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// Webpage Image Path
        /// </summary>
        public string wpip
        {
            get
            {
                return string.Format("~/wpi/{0}-{1}.jpeg", ws.ID, wp.ID);
            }
        }
        /// <summary>
        /// Website
        /// </summary>
        public Website ws { get; set; }
        /// <summary>
        /// Webpage
        /// </summary>
        public Webpage wp { get; set; }

        private string palletePath;

        public List<VisitActivity> Activities { get; set; }

        public HeatMapGenerator(string _palletePath)
        {
            palletePath = _palletePath;
        }

        
        public void GenerateMap()
        {
            if (!wia)
            {
                var htmlToImage = new NReco.ImageGenerator.HtmlToImageConverter();
                htmlToImage.GenerateImageFromFile(string.Format("http://{0}{1}?{2}", ws.Name, wp.Path, wp.QueryString), null, HttpContext.Current.Server.MapPath(string.Format("~/wpi/{0}-{1}.jpeg", ws.ID, wp.ID)));
            }
            Image img = Image.FromFile(HttpContext.Current.Server.MapPath(this.wpip));
            using (Graphics g = Graphics.FromImage(img))
            {
                using (SolidBrush transb = new SolidBrush(Color.FromArgb(180, 51, 51, 61)))//#33333D
                {
                    g.FillRectangle(transb, 0, 0, img.Width, img.Height);
                }

                foreach (var i2 in Activities)
                {
                    using (SolidBrush transb = new SolidBrush(Color.FromArgb(255, 255, 60, 0)))//#ff3c00
                    {
                        g.FillEllipse(transb, i2.MouseClickX.Value - 10, i2.MouseClickY.Value - 10, 10, 10);
                    }
                }
            }

            _hmpath = string.Format("~/heatmaps/{0}.png", Guid.NewGuid().ToString());
            img.Save(HttpContext.Current.Server.MapPath(_hmpath), System.Drawing.Imaging.ImageFormat.Png);

        }

        

        private double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        public double DistanceTo(Point point1, Point point2)
        {
            //var a = (double)(point2.X - point1.X);
            //var b = (double)(point2.Y - point1.Y);

            //return Math.Sqrt(a * a + b * b);

            // Calculating distance 
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) +
                          Math.Pow(point2.Y - point1.Y, 2) * 1.0);
        }

        

    }
}