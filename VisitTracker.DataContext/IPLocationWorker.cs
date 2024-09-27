using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VisitTracker.Models;

namespace VisitTracker.DataContext
{
    public class IPLocationWorker(Microsoft.Extensions.Configuration.IConfiguration config, VisitTrackerDBContext context)
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config = config;
        private readonly VisitTrackerDBContext _context = context;

        public async Task<IP2LocationResult> GetLocationAsync(string ipAddress)
        {
            //ipAddress = "100.42.175.255";
            var obj = GetLocationFromDB(ipAddress);
            if (obj != null)
            {
                return new IP2LocationResult(obj);
            }
            else
            {
                var hc = new HttpClient()
                {
                    BaseAddress = new Uri(string.Format("https://api.ip2location.io/?ip={0}&key={1}&format=json", ipAddress, _config["IP2LocationKey"]))
                };

                var result = await hc.GetFromJsonAsync<IP2LocationResult>("");
                if (result != null)
                {
                    _context.IP2Locations.Add(new IP2Location()
                    {
                        CityName = result.City_Name,
                        CountryCode = result.Country_Code,
                        CountryName = result.Country_Name,
                        IPAddress = ipAddress.ToLower(),
                        RegionName = result.Region_Name,
                        Response = result.Response,
                        IsProxy = result.Is_Proxy,
                        Latitude = result.Latitude.HasValue  ? result.Latitude.Value.ToString() : "",
                        Longitude = result.Longitude.HasValue ? result.Longitude.Value.ToString() : "",
                        TimeZone = result.Time_Zone,
                        ZipCode = result.Zip_Code
                    });
                    _context.SaveChanges();
                }
                return result ?? new IP2LocationResult();
            }
        }

        public IP2Location GetLocationFromDB(string ipAddress)
        {
            var item = _context.IP2Locations.FirstOrDefault(t => t.IPAddress == ipAddress.ToLower());
            return item;
        }
    }
}
