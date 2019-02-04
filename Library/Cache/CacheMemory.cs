using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Cache
{
    public class CacheMemory
    {
        private static ICore _core = new CoreObject();
        static CacheMemory()
        {
            _core.CacheDataStorage.SetKey(Config.ClientId);
        }
        public static void ClearCmsCache()
        {
            _core.CacheDataStorage.Remove(Config.CacheKeys.CmsInit);
            _core.CacheDataStorage.Remove(Config.CacheKeys.CmsBrandImages);
            _core.CacheDataStorage.Remove(Config.CacheKeys.CmsVideos);
            _core.CacheDataStorage.Remove(Config.CacheKeys.HrefLookup);
            _core.CacheDataStorage.Remove(Config.CacheKeys.Menu);
            foreach (var cacheCategoryItem in Config.CacheKeys.CmsCategoryList)
            {
                _core.CacheDataStorage.Remove(string.Format(Config.CacheKeys.Category, cacheCategoryItem));
            }
            

        }
        public static void ClearMenu()
        {
            _core.CacheDataStorage.Remove(Config.CacheKeys.HrefLookup);
            _core.CacheDataStorage.Remove(Config.CacheKeys.Menu);
        }
        public static T Get<T>(string key)
        {
            return _core.CacheDataStorage.Get<T>(key);
        }

        private static void Set(string key, object value, CacheItemPolicy policy = null)
        {
            _core.CacheDataStorage.Add(key, value, policy);
        }

        public static async Task SetAndExpiresMinutesAsync(string key, object value, int minutes = 5)
        {
            await Task.Run(() =>
            {
                var options = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddMinutes(minutes) };
                Set(key, value, options);
            });
        }

        public static async Task SetAndExpiresHoursAsync(string key, object value, int hours = 1)
        {
            await Task.Run(() =>
            {
                var options = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddHours(hours) };
                Set(key, value, options);
            });
        }
    }
}
