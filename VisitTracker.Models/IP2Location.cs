using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitTracker.Models
{
    public class IP2Location
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Response { get; set; } = string.Empty;
        [MaxLength(50)]
        public string CountryCode { get; set; } = string.Empty;
        [MaxLength(300)]
        public string CountryName { get; set; } = string.Empty;
        [MaxLength(300)]
        public string RegionName { get; set; } = string.Empty;
        [MaxLength(300)]
        public string CityName { get; set; } = string.Empty;
        [MaxLength(30)]
        public string IPAddress { get; set; } = string.Empty;
        public bool IsProxy { get; set; }

        public string ZipCode { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;

        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
    }

    public class IP2LocationResult
    {
        private string _response = string.Empty;
        public string Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value.Trim("-".ToCharArray());
            }
        }

        private string _countrycode = string.Empty;
        public string Country_Code
        {
            get { return _countrycode; }
            set
            {
                _countrycode = value.Trim("-".ToCharArray());
            }
        }

        private string _countryname = string.Empty;
        public string Country_Name
        {
            get { return _countryname; }
            set
            {
                _countryname = value.Trim("-".ToCharArray());
            }
        }

        private string _regionname = string.Empty;
        public string Region_Name
        {
            get { return _regionname; }
            set
            {
                _regionname = value.Trim("-".ToCharArray());
            }
        }

        private string _cityname = string.Empty;
        public string City_Name
        {
            get { return _cityname; }
            set
            {
                _cityname = value.Trim("-".ToCharArray());
            }
        }

        public string Zip_Code { get; set; } = string.Empty;
        public string Time_Zone { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool Is_Proxy { get; set; }

        public IP2LocationResult() { }

        public IP2LocationResult(IP2Location obj)
        {
            City_Name = obj.CityName;
            Region_Name = obj.RegionName;
            Country_Name = obj.CountryName;
            Country_Code = obj.CountryCode;
            Response = obj.Response;
            Zip_Code = obj.ZipCode;
            Time_Zone = obj.TimeZone;
            if (double.TryParse(obj.Latitude, out double d))
                Latitude = d;
            if (double.TryParse(obj.Longitude, out double d2))
                Longitude = d2;
            Is_Proxy = obj.IsProxy;

        }
    }
}
