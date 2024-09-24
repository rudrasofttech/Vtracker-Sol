using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using VisitTracker.Models;


namespace VisitTracker
{
    
    public class HeatMapGenerator(string _palletePath, string _webrootpath)
    {



        public string HeatMapPath{get; set;} =  string.Empty;

        public string WebrootPath { get; set; } = _webrootpath;

        /// <summary>
        /// Webpage Image Available
        /// </summary>
        public bool wia
        {
            get
            {
                return File.Exists(Path.Combine(WebrootPath, $"wpi/{ws.ID}-{wp.ID}.jpeg"));
            }
        }
        /// <summary>
        /// Webpage Image Path
        /// </summary>
        public string wpip
        {
            get
            {
                return $"{WebrootPath}/wpi/{ws.ID}-{wp.ID}.jpeg";
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

        private string palletePath = _palletePath;

        public List<VisitActivity> Activities { get; set; }

        public void GenerateMap()
        {
            if (!wia)
            {
                var htmlToImage = new NReco.ImageGenerator.HtmlToImageConverter();
                htmlToImage.GenerateImageFromFile($"http://{ws.Name}{wp.Path}?{wp.QueryString}", null, $"{WebrootPath}/wpi/{ws.ID}-{wp.ID}.jpeg");
            }
            System.Drawing.Image img = Image.FromFile(wpip);
            using (Graphics g = Graphics.FromImage(img))
            {
                using (var transBrush = new SolidBrush(Color.FromArgb(180, 51, 51, 61)))//#33333D
                {
                    g.FillRectangle(transBrush, 0, 0, img.Width, img.Height);
                }

                foreach (var i2 in Activities)
                {
                    if (i2.MouseClickX.HasValue && i2.MouseClickY.HasValue)
                    {
                        using var transb = new SolidBrush(Color.FromArgb(255, 255, 60, 0));//#ff3c00
                        g.FillEllipse(transb, x: i2.MouseClickX.Value - 10, y: i2.MouseClickY.Value - 10, 10, 10);
                    }
                }
            }

            HeatMapPath = $"{WebrootPath}/heatmaps/{Guid.NewGuid()}.png";
            img.Save(HeatMapPath, System.Drawing.Imaging.ImageFormat.Png);

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