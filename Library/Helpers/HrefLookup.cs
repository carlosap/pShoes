using MadServ.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Library.Cache;

namespace Library.Helpers
{
    public class HrefLookup
    {

        private static ICore _core { get; set; }
        private static readonly object ObjectLock = new object();
        public NameValueCollection Forward { get; set; }
        public NameValueCollection Reverse { get; set; }

        public HrefLookup()
        {
            Forward = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            Reverse = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        }

        public HrefLookup(string contents)
            : this()
        {
            var json = JsonConvert.DeserializeObject<dynamic>(contents);

            foreach (dynamic category in json.categories)
            {
                var url = category.pageURL.ToString();
                var id = category.id.ToString();

                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(id))
                {
                    var href = string.Format("{0}{1}", (url != null && !url.StartsWith(Config.Params.HrefPrefix) ? Config.Params.HrefPrefix : string.Empty), url);

                    if (Forward.Get(href) == null)
                        Forward.Add(href, id);

                    if (Reverse.Get(id) == null)
                        Reverse.Add(id, href);
                }
            }
        }

        public bool HasData
        {
            get
            {
                return Forward.Count > 0 && Reverse.Count > 0;
            }
        }

        public static HrefLookup Load(ICore core)
        {
            _core = core;
            var result = CacheMemory.Get<HrefLookup>(Config.CacheKeys.HrefLookup);
            if (result != null && result.HasData) return result;
            result = LoadFromDiskAndCache(Config.Params.HrefLookupDirectory);
            if (result != null || result.HasData)
            {
                CacheMemory.SetAndExpiresHoursAsync(Config.CacheKeys.HrefLookup, result, 4);
            }
            return result;
        }

        private static HrefLookup LoadFromDiskAndCache(string path)
        {
            var result = new HrefLookup();
            var directory = new DirectoryInfo(path);
            var fileInfo = directory.GetFiles().OrderByDescending(file => file.LastWriteTime).FirstOrDefault();

            // MJN: null reference, it is possible for there to be no element in 1
            if (fileInfo != null)
            {
                result = GetHrefLookupConfig(fileInfo, result);
            }
            else if (directory.GetFiles().Count() > 1)
            {
                var previousConfig = directory.GetFiles().OrderByDescending(file => file.LastWriteTime).ElementAt(1);

                result = GetHrefLookupConfig(previousConfig, result);
            }

            return result;
        }

        private static HrefLookup GetHrefLookupConfig(FileInfo fileInfo, HrefLookup result)
        {
            try
            {
                lock (ObjectLock)
                {
                    using (var reader = new StreamReader(fileInfo.FullName))
                    {
                        var contents = reader.ReadToEnd();
                        if (contents != string.Empty)
                        {
                            result = new HrefLookup(contents);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //allow a try on next user
                CacheMemory.ClearMenu();
                CacheMemory.ClearCmsCache();
                result = new HrefLookup();
            }
            return result;
        }
    }
}
