using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace Library.IRequests
{
    public static class StoreLocatorOnDemandWare
    {
        public static string MakeRequest(string url)
        {
            HttpWebResponse response;
            string responseText;

            if (RequestSend(out response, url))
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
            using (var responseStream = response.GetResponseStream())
            {
                var streamToRead = responseStream;
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                }
                using (var streamReader = new StreamReader(streamToRead, Encoding.UTF8))
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
                var request = (HttpWebRequest) WebRequest.Create(url);
                request.KeepAlive = true;
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.Headers.Add("X-Requested-With", @"XMLHttpRequest");
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
                request.Referer = "https://development-store-payless.demandware.net/s/payless/stores";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate, sdch");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,en-AU;q=0.6,en-CA;q=0.4");
                response = (HttpWebResponse) request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse) e.Response;
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