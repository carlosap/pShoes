using Enums;
using Library.RequestHandler;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Library.Cache;

namespace Library.Helpers
{
    public static class ProductPricing
    {
        private static ICore _core { get; set; }
        public static List<Price> GetByID(ICore core, string productID)
        {
            var result = new List<Price>();
            _core = core;

            var cacheKey = string.Format(Config.CacheKeys.ProductPricing, productID);
            result = CacheMemory.Get<List<Price>>(cacheKey);

            if (result == null || !result.Any())
            {
                var resultResponse = MakeRequest(productID);
                result = Parse(resultResponse);
                CacheMemory.SetAndExpiresHoursAsync(cacheKey, result, 1);
            }
            return result;
        }

        private static IResultResponse MakeRequest(string productID)
        {
            var url = string.Format(Config.Urls.GetProductPricing, productID);
            _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, new List<SiteError>())
            {
                OverrideBlockXDocumentConversion = true
            };
            return _core.RequestManager.Communicate(_core.CommunicationRequest);
        }

        private static List<Price> Parse(IResultResponse response)
        {
            var result = new List<Price>();
            var pricing = JsonConvert.DeserializeObject<dynamic>(response.RawData);
            if (pricing == null || pricing.prices == null) return result;
            if (decimal.Parse(pricing.prices.sale.ToString()) == 0)
            {
                var range = pricing.prices.rangePrice.ToString().Split('-');
                var low = range[0].Trim();
                var high = range[1].Trim();
                var priceToAdd = new Price(low) { Label = "range" };
                var highPrice = new Price(high);
                priceToAdd.Label = highPrice.Formatted;
                result.Add(priceToAdd);
            }
            else
            {
                var sale = new Price(pricing.prices.sale.ToString()) { Label = "sale" };
                result.Add(sale);
                if (pricing.prices.standard == pricing.prices.sale) return result;
                var reg = new Price(pricing.prices.standard.ToString()) { Label = "reg" };
                result.Add(reg);
            }

            return result;
        }
    }
}
