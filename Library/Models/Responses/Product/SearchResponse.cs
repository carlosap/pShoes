using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using Library.Models.Requests;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.RequestHandler;
using Enums;
using Newtonsoft.Json.Linq;
using Library.Helpers;
using Newtonsoft.Json;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "SearchResponse")]
    public class SearchResponse : IResponse
    {
        public ExtendedFilter Filters { get; set; }
        public Pager Pager { get; set; }
        public List<ProductListItem> Products { get; set; }
        public List<ProductRaw> ProductRaw{ get; set; }       
        public ExtendedSorter Sorter { get; set; }
        public string Breadcrumbs { get; set; }
        public string Href { get; set; }
        public string CategoryID { get; set; }
        public string Term { get; set; }
        public string ProductIds { get; set; }
        public List<SiteError> _errors { get; set; }
        public PaylessMadCms.PromoSlot Promo { get; set; }
        private bool _search { get; set; }
        private ICore _core { get; set; }

        public SearchResponse()
        {
            Filters = new ExtendedFilter();
            Pager = new Pager();
            Products = new List<ProductListItem>();
            Sorter = new ExtendedSorter();
            _errors = new List<SiteError>();
            ProductRaw = new List<ProductRaw>();
        }
        public PLPExtraInfo GetExtraPLPInfo(JArray products, string productid)
        {
            var result = new PLPExtraInfo();
            foreach (var cat in products)
            {
                if (cat["pid"].ToString() == productid)
                {
                    var sale = false;
                    var bogo = false;
                    var clearance = false;
                    foreach (string flag in cat["flags"])
                    {
                        switch (flag)
                        {
                            case "sale":
                                sale = true;
                                continue;
                            case "clearance":
                                clearance = true;
                                continue;
                            case "bogo":
                                bogo = true;
                                break;
                        }
                    }
                    result = new PLPExtraInfo() 
                    { 
                        defaultColor = cat["defaultColor"].ToString(), 
                        prices = new Prices() 
                        {
                            standard = FormatPrice(cat["prices"]["standard"].ToString()),
                            sale = FormatPrice(cat["prices"]["sale"].ToString()),
                            rangelow = FormatPrice(cat["prices"]["range"].ToString().Split('-')[0]),
                            rangehigh = FormatPrice(cat["prices"]["range"].ToString().Split('-')[1])
                        },
                        IsSale = sale,
                        IsBogo = bogo,
                        IsClearance = clearance
                    };

                    break;
                }
            }
            return result;
        }
        private string FormatPrice(string price)
        {
            return price.Trim().Replace("N/A", "").Replace("$", "");
        }
        public List<VariationAttribute> GetColorVariations(JArray products, string productId)
        {
            var allVariations = new List<VariationAttribute>();
            foreach (var cat in products)
            {
                if (cat != null && cat["pid"].ToString() != productId) continue;
                var values = new List<VariationAttributeValue>();
                if (cat != null)
                    foreach(var color in cat["variationColors"])
                    {                      
                        var n = ((string)color).ToCharArray();
                        n[0] = char.ToUpper(n[0]);
                        var name = new string(n); 
                        values.Add(new VariationAttributeValue { Name = name, Value = name.ToLower() });
                    }
                allVariations.Add(new VariationAttribute {Id = "color",Name = "Color",Values = values});
            }
            return allVariations;
        }
        public SearchResponse(ProductSearchResult searchResult, SearchRequest request, HrefLookup hrefLookup, ICore core) : this()
        {
            _core = core;
            if (searchResult.Count > 0)
            {
                Filters = new ExtendedFilter(searchResult.Refinements, searchResult.SelectedRefinements, hrefLookup);
                Sorter = new ExtendedSorter(searchResult.SortingOptions, searchResult.SelectedSortingOption);
                var catName = hrefLookup.Forward.Get(ParsingHelper.GetHrefWithoutQueryString(request.Href));
                var plpInfo = GetAdditionalPlpInfo(request, catName);
                foreach (var product in searchResult.Hits.Where(a => a.Price != Convert.ToDecimal(Config.Params.PriceToExclude)).ToList())
                    Products.Add(new ProductListItem(product, GetExtraPLPInfo(plpInfo, product.ProductId), GetColorVariations(plpInfo, product.ProductId)));

                Pager.PageSize = request.PageSize;
                Pager.CurrentPage = request.Page;
                Pager.RecordCount = searchResult.Count;
                Pager.TotalRecords = searchResult.Total;
                Pager.TotalPages = (int)Math.Ceiling((double)Pager.TotalRecords / Pager.PageSize);
                ProductIds = string.Join(",", Products.Select(x => x.ProductId));
                Breadcrumbs = string.Join(" | ", Filters.Path.Select(x => x.Key.ToLowerInvariant()));
            }
            else
            {
                Breadcrumbs = "We're sorry, no products were found for your search";
            }
            
            Term = searchResult.Query;
            Href = request.Href;
        }

        private JArray GetAdditionalPlpInfo(SearchRequest request, string categoryName)
        {
            JArray result;
            var comRequest = BuildAdditionalPlpRequest(request, categoryName);
            try
            {
                result = RequestJArray(comRequest);
            }
            catch
            {
                result = RequestJArray(comRequest);
            }
            return result;
        }
        private JArray RequestJArray(ICommunicationRequest req)
        {
            try
            {
                var results = RequestJArrayCache(req);
                if (results != null && results.Count > 1) return results;
                req.Url = req.Url.Replace(Config.Urls.GetAdditionalPLPDataCache, Config.Urls.GetAdditionalPLPData); //-->fallback code- starts here.
                var data = _core.RequestManager.Communicate(req).RawData;
                return !string.IsNullOrWhiteSpace(data) ? JArray.Parse(data) : new JArray();
            }
            catch (Exception)
            {
                return new JArray();
            }
        }
        private JArray RequestJArrayCache(ICommunicationRequest req)
        {
            var results = new JArray();
            try
            {
                var extendedCacheList = new List<string>();
                var data = _core.RequestManager.Communicate(req).RawData;
                data = data.TrimStart('[').TrimEnd(']');
                var dataArray = data.Split(new[] {"],["}, StringSplitOptions.None).ToList();
                if (!dataArray.Any()) return results;
                extendedCacheList.AddRange(dataArray.Select(item => item));
                foreach (var token in extendedCacheList)
                {
                    try
                    {
                        results.Add(JsonConvert.DeserializeObject(token));
                    }
                    catch (Exception)
                    {
                        //DO NOT REMOVE - MAL-FORMATTED JSON ERRORS ON 
                        //LARGE COLLECTIONS>
                        continue;
                    }
                }
             
            }
            catch (Exception)
            {
                //return empty to trigger fallback code.
                return new JArray();
            }
            return results;
        }

        private ExtendedComRequest BuildAdditionalPlpRequest(SearchRequest request, string category)
        {
            var url = Config.Urls.GetAdditionalPLPDataCache;
            if (category != null && category.ToLower().Contains("salesandclearance"))
            {
                if (url != null) url += "?cgid=" + category.Split('-')[1] + "&pmid=saleandclearance";
            }
            else
            {
                if (request.Href.Contains("search"))
                {
                    var query = request.Href.Substring(request.Href.LastIndexOf("=") + 1);
                    url += "?q=" + query;
                    _search = true;
                }
                else
                {
                    url += "?cgid=" + category;
                }
            }
            return new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
            {
                OverrideBlockXDocumentConversion = true
            };
        }

    }


    public class ProductRaw
    {
        public string name { get; set; }
        public string pid { get; set; }
        public List<object> flags { get; set; }
        public string defaultColor { get; set; }
        public List<string> variationColors { get; set; }
        public string lotNumber { get; set; }
        public Prices prices { get; set; }
    }


}
