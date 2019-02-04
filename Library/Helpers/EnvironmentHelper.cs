using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace Library.Helpers
{
    public static class EnvironmentHelper
    {
        public static string GetMobileBaseUrl(HttpRequestBase httpRequest, bool isSecure = false)
        {
            var mobileBaseUrl = Config.Urls.DefaultMobileBaseUrl;
            if (httpRequest != null)
                if (httpRequest.UrlReferrer != null)
                    return httpRequest.UrlReferrer.GetLeftPart(UriPartial.Authority).ToLowerInvariant();

            mobileBaseUrl = isSecure
                ? mobileBaseUrl.Replace("http://", "https://")
                : mobileBaseUrl.Replace("https://", "http://");
            return mobileBaseUrl;
        }

        public static bool UseProductionAPI(HttpRequestBase httpRequest)
        {
            var mobileBaseUrl = GetMobileBaseUrl(httpRequest);
            return !(mobileBaseUrl.Contains(".ms.") || mobileBaseUrl.Contains(".dev."));
        }

        public static int GetEnvironmentId(HttpRequestBase httpRequest)
        {
            var result = Config.EnvironmentIdLookup.Last().Value;
            var mobileBaseUrl = GetMobileBaseUrl(httpRequest);
            foreach (var key in Config.EnvironmentIdLookup.Keys)
            {
                if (!mobileBaseUrl.Contains(key)) continue;
                result = Config.EnvironmentIdLookup[key];
                break;
            }
            return result;
        }

        public static string GetLastIPAddressOctet()
        {
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress.GetAddressBytes()[3].ToString();
        }

        public static bool IsDev(HttpContextBase context)
        {
            if (context.Request.Url == null) return true;
            return ((context.Request.Url.ToString().Contains("dev") || context.Request.Url.ToString().Contains(".mm.") ||
                     context.Request.Url.ToString().Contains(".ms.")));
        }

        public static bool IsStaging(HttpContextBase context)
        {
            return (context != null && (context.Request.Url.ToString().Contains("staging")));
        }

        public static bool IsCS1(HttpContextBase context)
        {
            return (context != null && (context.Request.Url.ToString().Contains("cs1")));
        }

        public static bool IsUAT(HttpContextBase context)
        {
            return (context != null && (context.Request.Url.ToString().Contains("uat")));
        }

        public static bool IsDevOrStaging(HttpContextBase context)
        {
            return IsDev(context) || IsStaging(context);
        }

        public static bool IsProd()
        {

#if (PRODUCTION)
            return true;
#endif
            return false;
        }

        public static string GetCmsEnvironment(HttpContextBase context)
        {
            var result = "staging";

#if (PRODUCTION)
            result = "";
#endif

            return result;
        }
    }
}
