using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Enums;
using Library.Cache;
using Library.DemandWare.Models;
using Library.DemandWare.Models.DTOs;
using Library.DemandWare.RequestManager;
using Library.Extensions;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.Models.PowerReview;
namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "Search", RequestType = typeof(SearchRequest),
        ResponseType = typeof(Response<SearchResponse>))]
    public class Search : IRequest
    {
        private SearchRequest _request;
        private HrefLookup _hrefLookup;
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private bool IsViewAllProductSearch { get; set; }
        private bool IsEmailCamPaign { get; set; }
        public List<AvgRate> PowerReviews { get; set; }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<SearchResponse>();
            _request = (SearchRequest) parameters;

            try
            {
                if (_request.Href != null)
                {
                    var cacheKey = string.Format(Config.CacheKeys.Search, _request.Href, _request.Page, _request.PageSize);
                    IsViewAllProductSearch = _request.Href.Contains("view-all");                  
                    IsEmailCamPaign = _request.Href.ToLower().Contains("utm_medium=email");                  
                    result = CacheMemory.Get<Response<SearchResponse>>(cacheKey);
                    if (result.resultset != null && string.IsNullOrWhiteSpace(result.resultset.Href))
                    {
                        _hrefLookup = HrefLookup.Load(_core);
                        var config = BuildAPIConfig(parameters);

                        if (config != null) 
                            result = GetResponse(config);

                        if (result.resultset.Filters != null && result.resultset.Filters.FilterSections != null)
                            result.resultset.Filters.FilterSections = SelectSpecifiedSize(result.resultset.Filters.FilterSections);

                        var href = ParsingHelper.GetHrefWithoutQueryString(_request.Href);
                        result.resultset.CategoryID = _hrefLookup.Forward.Get(href);
                        var resultVideos = CacheMemory.Get<Dictionary<string, PaylessMadCms.PromoSlot>>(Config.CacheKeys.CmsVideos);
                        if (resultVideos != null && resultVideos.Keys.Contains(href))
                        {
                            result.resultset.Promo = resultVideos[href];
                        }

                        if (!_errors.Any() && result.resultset.Products.Any())
                        {
                            CacheMemory.SetAndExpiresMinutesAsync(cacheKey, result, 15);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(string.Format("Search.Execute({0}):{1}-{2}", ex.LineNumber(), ex.Message, ex.StackTrace), ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private DWClientConfig BuildAPIConfig(IRequestParameter parameters)
        {
            var sb = new StringBuilder();
            try
            {
                var href = _request.Href;
                const string hrefRegex = @"^(?<href>[^?#]+)(\?(?<query>[^#]+))*(#(?<hash>.+))*";

                if (string.IsNullOrEmpty(href))
                {
                    return new DWClientConfig(_core)
                    {
                        Path = Config.DWPath.Search,
                        Query = sb.ToString(),
                        Method = HttpMethod.GET
                    };
                }

                var sortBy = "best-matches";
                var searchTerm = string.Empty;
                var categoryId = string.Empty;
                var promotionId = string.Empty;
                var queryString = string.Empty;
                var hashString = string.Empty;
                var rootHref = string.Empty;
                var start = _request.PageSize * (_request.Page - 1);
                var refinerIndexOffset = 1;

                href = href.Replace("%27", "'")
                    .Replace("%7C", "|")
                    .Replace("%7c", "|");

                sb.AppendFormat("count={0}", _request.PageSize);
                sb.AppendFormat("&start={0}", start);
                sb.Append("&expand=images,prices");

                var match = Regex.Match(href, hrefRegex, RegexOptions.Compiled);
                if (match.Success)
                {
                    rootHref = match.Groups["href"].Value
                        .ToLowerInvariant();
                    queryString = match.Groups["query"].Value;
                    hashString = match.Groups["hash"].Value;
                }

                if (rootHref.EndsWith("/"))
                    rootHref = rootHref.Remove(rootHref.Length - 1, 1);

                if (!rootHref.EndsWith("/search"))
                    categoryId = _hrefLookup.Forward.Get(rootHref);


                // Category Refiner
                if (!string.IsNullOrEmpty(categoryId))
                    sb.AppendFormat("&refine_{0}=cgid={1}", refinerIndexOffset++,
                        categoryId.Replace("-girl-viewall", "-girls").Replace("-viewall", string.Empty));


                // Decipher Query
                if (!string.IsNullOrEmpty(queryString))
                {
                    var queryNvc = HttpUtility.ParseQueryString(queryString);
                    searchTerm = queryNvc.Get("q") ?? searchTerm;
                    promotionId = queryNvc.Get("pmid") ?? promotionId;
                    foreach (var key in queryNvc.AllKeys)
                    {
                        if (!key.StartsWith("prefn")) continue;
                        var refinerName = queryNvc.Get(key);
                        var index = 0;
                        if (!int.TryParse(key.Replace("prefn", string.Empty), out index)) continue;
                        index += refinerIndexOffset;
                        if (index >= 10) continue;
                        var refinerValue = queryNvc.Get(key.Replace("n", "v"));
                        if (!string.IsNullOrEmpty(refinerValue))
                            sb.AppendFormat("&refine_{0}={1}={2}", index, refinerName, refinerValue);

                    }
                }
                // Promotion Refiner
                if (!string.IsNullOrEmpty(promotionId))
                    sb.AppendFormat("&refine_{0}=pmid={1}", refinerIndexOffset++, promotionId);


                // Decipher Hash
                if (!string.IsNullOrEmpty(hashString))
                {
                    var hashNvc = HttpUtility.ParseQueryString(hashString);
                    var priceMin = hashNvc.Get("pmin");
                    var priceMax = hashNvc.Get("pmax");
                    sortBy = hashNvc.Get("srule") ?? sortBy;
                    searchTerm = hashNvc.Get("q") ?? searchTerm;
                    // Price Refiner
                    if (!string.IsNullOrEmpty(priceMin) && !string.IsNullOrEmpty(priceMax))
                        sb.AppendFormat("&refine_{0}=price=({1}..{2})", refinerIndexOffset++, priceMin, priceMax);

                    // Sorter
                    sb.AppendFormat("&sort={0}", sortBy);
                    sb.AppendFormat("&srule={0}", sortBy);
                    // All Other Refiners
                    foreach (var key in hashNvc.AllKeys)
                    {
                        if (!key.StartsWith("prefn")) continue;
                        var refinerName = hashNvc.Get(key);
                        var index = 0;
                        if (!int.TryParse(key.Replace("prefn", string.Empty), out index)) continue;
                        index += refinerIndexOffset;
                        if (index >= 10) continue;
                        var refinerValue = hashNvc.Get(key.Replace("n", "v"));
                        if (!string.IsNullOrEmpty(refinerValue))
                            sb.AppendFormat("&refine_{0}={1}={2}", index, refinerName, refinerValue);

                    }
                }

                //Custom Champion Payless list to search for
                //champion products
                if (isChampionRedirect(href.Replace("/", "")) || isChampionRedirect(searchTerm))
                {
                    searchTerm = "champion";
                }

                // Search Term - From Hash or Query
                if (!string.IsNullOrEmpty(searchTerm))
                    sb.AppendFormat("&q={0}", searchTerm);


            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(string.Format("Search.BuildAPIConfig({0}):{1}-{2}", ex.LineNumber(), ex.Message,ex.StackTrace), ErrorSeverity.FollowUp, ErrorType.RequestError));
                return null;
            }

            return new DWClientConfig(_core)
            {
                Path = Config.DWPath.Search,
                Query = sb.ToString(),
                Method = HttpMethod.GET
            };
            
        }

        private bool isChampionRedirect(string searchTerm)
        {
            try
            {
                return Config.ChampionSearchRedirect.Exists(x => x.ToLower() == searchTerm.ToLower());
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
        private Response<SearchResponse> GetResponse(DWClientConfig config)
        {
            var result = new Response<SearchResponse>();
            try
            {
                var apiResponse = DWClient.GetAPIResponse<ProductSearchResult>(config);
                if (apiResponse.ResponseStatus.Code == HttpStatusCode.OK)
                {
                    result.resultset = new SearchResponse(apiResponse.Model, _request, _hrefLookup, _core);
                    result.template = Config.ResponseTemplates.ProductList;
                    if (!string.IsNullOrEmpty(result.resultset.ProductIds))
                    {
                        PowerReviews = GetPwAvgRate(result.resultset.ProductIds);
                        var colors = new List<string>();
                        var colorsFilter = result.resultset.Filters.AppliedFilterSections.Find(x => x.Note == "c_color");
                        if (colorsFilter != null)
                        {
                            colorsFilter.FilterOptions.ForEach(
                                option => colors.Add(option.Value.ToLowerInvariant().Replace("/", "_").Replace(" ", "")));
                        }
                        var productListExtension = new ProductListExtension(_core, _errors);
                        var productListExtensionRequest = new ProductListExtensionRequest
                        {
                            ProductsIds = result.resultset.ProductIds,
                            Colors = colors,
                            Products = result.resultset.Products
                        };
                        productListExtension._usePredefinedVariations = !IsViewAllProductSearch;
                        var productListExtensionResponse = productListExtension.Execute(productListExtensionRequest);
                        var productIdToExtension =
                            ((Response<ProductListExtensionResponse>)productListExtensionResponse).resultset
                                .ProductIdToExtension;
                        if (productIdToExtension.Any())
                        {
                            result.resultset.Products.ForEach(p =>
                            {
                                if (!productIdToExtension.ContainsKey(p.ProductId)) return;
                                var extension = productIdToExtension[p.ProductId];
                                if (extension == null) return;
                                var avgRate = GetPowerRewiewAverageRateById(p.ProductId);
                                if (avgRate > 0) extension.Rating = avgRate;
                                if (string.IsNullOrEmpty(p.Image.Src) || colors.Count > 0) p.Image = extension.Image;
                                p.Description = extension.Brand;
                                p.Notes = extension.ItemFeatures;
                                p.Rating = extension.Rating;
                                p.IsAvailableInMultipleColors = extension.IsAvailableInMultipleColors;
                                p.AvailableVariations = extension.AvailableVariations;
                                p.ProductFlags = Product.LookupCallouts(extension.ProductFlags);
                                p.CallOuts = extension.Callout;
                            });
                        }
                    }
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.GenericError, apiResponse.ResponseStatus.Message)
                    });
                }

            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(string.Format("Search.GetResponse({0}):{1}-{2}", ex.LineNumber(), ex.Message, ex.StackTrace), ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private List<ExtendedFilterGrouping> SelectSpecifiedSize(List<ExtendedFilterGrouping> filterSections)
        {
            var result = filterSections;
            try
            {
                var queryString = HttpUtility.ParseQueryString(_request.Href);
                var size = queryString.Get("prefv1");
                if (string.IsNullOrEmpty(size)) return result;
                var sizes = filterSections.FirstOrDefault(option => option.Note == "c_size");
                if (sizes == null) return result;
                {
                    if (sizes.FilterOptions == null) return result;
                    var selectedSize = sizes.FilterOptions.FirstOrDefault(option => option.Value == size);
                    if (selectedSize != null)
                        selectedSize.IsSelected = true;

                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(string.Format("Search.SelectSpecifiedSize({0}):{1}-{2}", ex.LineNumber(), ex.Message, ex.StackTrace), ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private decimal GetPowerRewiewAverageRateById(string productId)
        {
            decimal result = 0;
            try
            {
                if (PowerReviews.Any())
                {
                    foreach (var reviewItem in PowerReviews)
                    {
                        if (!reviewItem.page_id.ToLower().Trim().Equals(productId.ToLower().Trim())) continue;
                        result = decimal.Parse(reviewItem.average_rating);
                        break;
                    }
                }
            }
            catch (Exception)
            {
                /*igore. we will fallback on DW*/
            }
            return result;
        }

        private List<AvgRate> GetPwAvgRate(string productId)
        {
            var avgRatesRequest = new PowerReviewAvgRateRequest();
            var avgrates = new PowerReviewGetAvgRates(_core);
            avgRatesRequest.ProductId = productId;
            return ((Response<PowerReviewAvgRateResponse>)avgrates.Execute(avgRatesRequest)).resultset.AvgRates;
        }
    }
}