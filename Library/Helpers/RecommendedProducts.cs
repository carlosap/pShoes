using System;
using System.Collections.Generic;
using System.Linq;
using MadServ.Core.Models;
using MadServ.Core.Interfaces;
using Library.RequestHandler;
using Enums;
using System.Xml.Linq;
using Library.Cache;
using MadServ.Core.Extensions;

namespace Library.Helpers
{
    public class RecommendedProducts
    {
        public List<KeyValuePair<string, string>> Breadcrumb { get; set; }
        public List<RecommendedProduct> Products { get; set; }

        public static RecommendedProducts Load(string productID, ICore core)
        {
            var result = new RecommendedProducts();

            var cacheKey = string.Format(Config.CacheKeys.RecommendProducts, productID); 
            result = CacheMemory.Get<RecommendedProducts>(cacheKey);

            if (result == null || result.Products == null)
            {
                var request = GetRequest(productID, core);
                var response = core.RequestManager.Communicate(request);
                result.Products = ParseRecommendedProducts(response.XDocument);
                result.Breadcrumb = ParseBreadcrumb(productID, response.XDocument);
                CacheMemory.SetAndExpiresHoursAsync(cacheKey, result, 1);
            }
            return result;
        }

        private static ICommunicationRequest GetRequest(string productID, ICore core)
        {
            var url = Config.Urls.GetAdditionalPDPData + productID;

            core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, core, new List<SiteError>());

            return core.CommunicationRequest;
        }

        private static List<RecommendedProduct> ParseRecommendedProducts(XDocument xdoc)
        {
            return xdoc.Descendants("div")
                .WhereAttributeEquals("class", "recommendation")
                .Select(a => new RecommendedProduct()
                {
                    name = a.Descendants("span").WhereAttributeEquals("class", "name").Select(b => b.Value).FirstOrDefault(),
                    ID = a.Descendants("span").WhereAttributeEquals("class", "Id").Select(b => b.Value).FirstOrDefault(),
                    pvmColor = a.Descendants("span").WhereAttributeEquals("class", "pvmColor").Select(b => b.Value).FirstOrDefault(),
                    prodURL = a.Descendants("span").WhereAttributeEquals("class", "prodURL").Select(b => b.Value).FirstOrDefault().Replace("https://www.payless.com", "").Replace("http://www.payless.com", ""),
                    imgURL = a.Descendants("span").WhereAttributeEquals("class", "imgURL").Select(b => b.Value).FirstOrDefault(),
                    imgALT = a.Descendants("span").WhereAttributeEquals("class", "imgALT").Select(b => b.Value).FirstOrDefault(),
                    imgTitle = a.Descendants("span").WhereAttributeEquals("class", "imgTitle").Select(b => b.Value).FirstOrDefault(),
                    Pricing = GetRecommendedProductPricing(a)
                }).ToList();
        }

        private static List<Price> GetRecommendedProductPricing(XElement x)
        {
            List<Price> lstPricing = new List<Price>();
            var standard = x.Descendants("span").WhereAttributeEquals("class", "standard").Select(b => b.Value).FirstOrDefault();
            var sale = x.Descendants("span").WhereAttributeEquals("class", "sale").Select(b => b.Value).FirstOrDefault();
            var range = x.Descendants("span").WhereAttributeEquals("class", "rangePrice").Select(b => b.Value).FirstOrDefault();
            var price = new Price();

            if (range.Contains("-") && range.Split('-')[0].Trim() != range.Split('-')[1].Trim())
            {
                price = new Price(range.Split('-')[0]);
                price.Label = range.Split('-')[1];
                lstPricing.Add(price);
            }
            else
            {
                price = new Price(sale);
                lstPricing.Add(price);
                price = new Price(standard);
                lstPricing.Add(price);
            }

            return lstPricing;
        }

        private static List<KeyValuePair<string, string>> ParseBreadcrumb(string id, XDocument xDoc)
        {
            var result = new List<KeyValuePair<string, string>>();

            var titles = xDoc.Descendants("span").WhereAttributeEquals("class", "title").ToList();
            var paths = xDoc.Descendants("span").WhereAttributeEquals("class", "path").ToList();
            var i = 0;
            var path = string.Empty;
            foreach (var t in titles)
            {
                path = paths[i].ToString().Contains("/payless") ? paths[i].Value.Split(new string[] { "/payless" }, StringSplitOptions.None)[1] : paths[i].Value;
                path = t.Value == "Home" ? "/" : path;
                result.Add(new KeyValuePair<string, string>(t.Value, path));
                i++;
            }

            return result;
        }
}

    public class RecommendedProduct
    {
        public string name { get; set; }
        public string ID { get; set; }
        public string onclick { get; set; }
        public object brand { get; set; }
        public string pvmColor { get; set; }
        public string prodURL { get; set; }
        public string imgURL { get; set; }
        public string imgALT { get; set; }
        public string imgTitle { get; set; }
        public IList<Price> Pricing { get; set; }

        public RecommendedProduct()
        {
            brand = new object();
        }
    }
}
