using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Library.IRequests
{
    public static class CartApplyCouponOnDemandWare
    {

        public static string MakeRequest(string url)
        {
            HttpWebResponse response;
            string responseText;

            if (RequestSend(out response,url))
            {
                responseText = ReadResponse(response);
                response.Close();
            }
            else
            {
                responseText = "Request Failed";
            }
            return responseText;
        }

        private static string ReadResponse(HttpWebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                Stream streamToRead = responseStream;
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                }
                using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static bool RequestSend(out HttpWebResponse response, string url)
        {
            response = null;
            try
            {
                string hostEnvPath = Config.Urls.CartApplyCouponOnDemandWare;
                string hostEnv = Config.Urls.Domain;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.Headers.Add("X-Requested-With", @"XMLHttpRequest");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
                //request.Referer = string.Format("https://{0}{1}",hostEnv,hostEnvPath);
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate, sdch");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,en-AU;q=0.6,en-CA;q=0.4");
                //request.Headers.Set(HttpRequestHeader.Cookie, @"dw=1; sid=TFHuafBYWy4G9BIhLyLGkAu0PXNGaI4IwUA; dwanonymous_04e5298ec7a0ace2a94023dc220fc92c=cfayBgEFqQfKwNWiVgKzbdcZTp; dwsid=esSyv0e4IKbg9S74NbuIvD0hYaXxiPWAJ0EB9qNhkFyiI7YX9pyHI3_H7fwiyOiPvOrI37Hp6MsQzg8laxrRmg==; __sonar=14771658346543747384; dwpersonalization_04e5298ec7a0ace2a94023dc220fc92c=bdz5AiaagG97waaadoqXoxLlib20151204; invodoSession=0ke_frba84CEZLS6DuOgDE; invodoVisitor=Bw8V93PzTV07w1T32MfCDN; dw=1; __utmt=1; s_sq=%5B%5BB%5D%5D; dwsecuretoken_04e5298ec7a0ace2a94023dc220fc92c=2g-LqBOzVW2wIQEjZChiiGrkfwYgsW-8ew==; mmcore.tst=0.782; mmid=-2058377695%7CBwAAAAoouyCXngwAAA%3D%3D; mmcore.pd=-1463037953%7CBwAAAAoBQii7IJeeDF5ZWZwCAP4mI0sd5tJIDwAAAKCEinYY5tJIAAAAAP//////////AAZEaXJlY3QBngwCAAAAAAAAAAAAAMt+AAD//////////wEA/z4AAABoqQ+pngwA/////wGeDJ4M//8BAAABAAAAAAGLnAAACfkAAAHLfgAAAQAAAAAAAAFF; mmcore.srv=nycvwcgus05; __utma=238895938.1809227582.1446752226.1446752226.1446754189.2; __utmb=238895938.4.10.1446754189; __utmc=238895938; __utmz=238895938.1446752226.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); rr_rcs=eF5jYSlN9rAwsUxLSUw00E0zNjXTNTExStY1tTBI0bU0MzUxNjM1tDC2SOHKLSvJTOGzNDLVNdQ1BAB-wg2G; mp_dev_mixpanel=%7B%22distinct_id%22%3A%20%22150d9270c05bc-0010b5274-671d127a-1fa400-150d9270c06182%22%7D; __CT_Data=gpv=5&apv_16455_www03=10; WRUID=0; s_vi=[CS]v1|2B1DD824051D27B1-600001092017C5FB[CE]; __ar_v4=Y4FOSUDHERF6XOFFW4IETD%3A20151105%3A8%7CUUBKHUOAZBF7LM2PX3ZXST%3A20151105%3A8%7CM5FQXOTUGZG6RGZDAQF4XC%3A20151105%3A8; s_pers=%20s_fid%3D484BA6B95735A3CD-2CB7D8D08C26F744%7C1509912755710%3B%20s_vs%3D1%7C1446756155711%3B%20gpv%3Dcheckout%253A%2520shipping%7C1446756155715%3B%20s_nr%3D1446754355718-Repeat%7C1478290355718%3B%20s_dl%3D1%7C1446756155721%3B; s_sess=%20ttcp%3D1446752226345%3B%20s_cpc%3D0%3B%20s_ppv%3D-%252C30%252C30%252C979%3B%20s_cc%3Dtrue%3B%20SC_LINKS%3D%3B%20s_sq%3Dcbipaylessdev%253D%252526pid%25253Dcheckout%2525253A%25252520shipping%252526pidt%25253D1%252526oid%25253Ddwfrm_cart_addCoupon%252526oidt%25253D3%252526ot%25253DSUBMIT%3B");
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }
            return true;
        }
    }
}

