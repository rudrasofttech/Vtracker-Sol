using System;
using System.Web;

namespace VTracker
{
    public class CacheManager
    {

        public static void Remove(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

        public static T Get<T>(string key)
        {
            try
            {
                return (T)HttpContext.Current.Cache.Get(key);
            }
            catch
            {
                return default(T);
            }
        }

        public static void Add(string key, object value, DateTime expiry)
        {
            HttpContext.Current.Cache.Insert(key, value, null, expiry, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
        }

        public static void AddSliding(string key, object value, int waitInMinutes)
        {
            HttpContext.Current.Cache.Insert(key, value, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(waitInMinutes));

        }
    }
}