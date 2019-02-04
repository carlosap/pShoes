using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using Enums;
using Library.Cache;
using Library.DemandWare.Models;
using Library.DemandWare.Models.DTOs;
using Library.DemandWare.RequestManager;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.PowerReview;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.Models.PowerReview;
using Newtonsoft.Json;
namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ProductDetail",
        RequestType = typeof(ProductDetailRequest), ResponseType = typeof(Response<ProductDetailResponse>))]
    public class ProductDetail : IRequest
    {
        private ProductDetailRequest _request;
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public ProductDetail()
        {
            _errors = new List<SiteError>();
        }
        public ProductDetail(ICore core)
            : this()
        {
            _core = core;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<ProductDetailResponse>();
            try
            {
                _request = (ProductDetailRequest) parameters;
                var cacheKey = string.Format(Config.CacheKeys.ProductDetails, _request.Id, _request.Color);
                result = CacheMemory.Get<Response<ProductDetailResponse>>(cacheKey);
                if (result == null || string.IsNullOrWhiteSpace(result.resultset.Product.ProductId))
                {
                    var config = BuildAPIConfig(parameters);
                    result = GetResponse(config);
                    if (!result.errors.Any() && !string.IsNullOrEmpty(result.resultset.Product.ProductId))
                    {
                       CacheMemory.SetAndExpiresHoursAsync(cacheKey, result, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ProductDetail.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private DWClientConfig BuildAPIConfig(IRequestParameter parameters)
        {
            var clientConfig = new DWClientConfig(_core);
            var path = string.Format("{0}/{1}", Config.DWPath.ProductDetail, _request.Id);
            try
            {                
                var sb = new StringBuilder();
                sb.Append("expand=options,images,prices,variations,availability,promotions");
                //Per OCAPI 17.2 - Added all_images=true to Display Image_Groups
                sb.Append("&all_images=true");
                clientConfig.Path = path;
                clientConfig.Query = sb.ToString();
                clientConfig.Method = HttpMethod.GET;
                return clientConfig;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(string.Format("Product.BuildAPIConfig ({0})", path), ErrorSeverity.FollowUp, ErrorType.RequestError));
                return clientConfig;
            }          
        }

        private Response<ProductDetailResponse> GetResponse(DWClientConfig config)
        {
            var result = new Response<ProductDetailResponse>();
            try
            {
                

                var apiResponse = DWClient.GetAPIResponse<DWProduct>(config);
                if (apiResponse.ResponseStatus.Code == HttpStatusCode.OK)
                {
                    result.resultset = new ProductDetailResponse(apiResponse.Model, _request.Color, _core, _errors);
                    try
                    {
                        result.resultset.RecommendedProducts = RecommendedProducts.Load(result.resultset.Product.ProductId, _core).Products;
                    }
                    catch
                    {
                        // ignored
                    }
                    result.template = Config.ResponseTemplates.ProductDetail;
                    var reviewsObj = GetPwReviews(result.resultset.Product.ProductId);
                    result.resultset.Product.PowerReview.Reviews = reviewsObj.Item1;
                    result.resultset.Product.PowerReview.Pagination = reviewsObj.Item2;
                    result.resultset.Product.PowerReview.Snapshot = GetPwSnapShots(result.resultset.Product.ProductId);
                    result.resultset.Product.PowerReview.MsqcTags = PowerReviewHelper.GetMsqcsTagSummary(result.resultset.Product.PowerReview.Reviews);
                    result.resultset.Product.PowerReview.FaceOff = GetPwFaceOff(result.resultset.Product.ProductId);
                    if (!string.IsNullOrWhiteSpace(result.resultset.Product.PowerReview.Snapshot.average_rating))
                    {
                        result.resultset.Product.ProductRating.Rating = decimal.Parse(result.resultset.Product.PowerReview.Snapshot.average_rating);
                    }                                            
                    if (result.resultset.Product.VariantIdsSegments.Count > 0)
                    {
                        IEnumerable<KeyValuePair<string, ProductDetailItemExtension>> tempDictionary =  new Dictionary<string, ProductDetailItemExtension>();
                        result.resultset.Product.VariantIdsSegments.ForEach(segment =>
                        {
                            var variantIdToExtension = GetDetailExtension(segment);
                            tempDictionary = tempDictionary.Union(variantIdToExtension);
                        });

                        if (!_errors.Any())
                        {
                            var completeDictionary = tempDictionary.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First().Value);

                            if (completeDictionary.Any())
                            {
                                result.resultset.Product.Colors.ForEach(color =>
                                {
                                    color.Sizes.ForEach(size =>
                                    {
                                        if (completeDictionary.ContainsKey(size.Value))
                                        {
                                            size.SizeSegment = completeDictionary[size.Value].SizeSegment;
                                            size.ExclusiveMsg = completeDictionary[size.Value].ExclusiveMsg;
                                            size.PriceRange = completeDictionary[size.Value].PriceRange;

                                        }
                                        color.SizeSegments = color.Sizes
                                            .Where(x => !string.IsNullOrEmpty(x.SizeSegment))
                                            .GroupBy(x => x.SizeSegment)
                                            .Select((x, i) => new Option
                                            {
                                                
                                                Name = x.First().SizeSegment,
                                                Value = x.First().SizeSegment,
                                                IsSelected = i == 0
                                            })
                                            .ToList();
                                    });
                                    color.Sizes = color.Sizes;
                                });                       
                            }
                        }
                    }
                    result.resultset.Product.Pricing = ProductPricing.GetByID(_core, _request.Id);
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
                var titleMsg = string.Format("ProductDetail.GetResponse - DW Productid: {0}-{1}", result.resultset.Product.ProductId, config.Path);
                _errors.Add(ex.Handle(titleMsg, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private Snapshot GetPwSnapShots(string productId)
        {
            var snapshotResponse = new PowerReviewSnapshotResponse();
            var snapshotRequest = new PowerReviewSnapshotRequest();
            var snapshot = new PowerReviewGetSnapshot(_core);        
            snapshotRequest.ProductId = productId;
            return ((Response<PowerReviewSnapshotResponse>)snapshot.Execute(snapshotRequest)).resultset.Snapshot; 
        }

        private Tuple<List<ReviewItem>,Pagination> GetPwReviews(string productId)
        {
            var reviewsResponse = new PowerReviewsResponse();
            var reviewsRequest = new PowerReviewsRequest();
            var reviews = new PowerReviewGetReviews(_core);
            reviewsRequest.ProductId = productId;
            reviewsRequest.PageNumber = 0; //API is "zero" base
            reviewsRequest.Sort = "desc";
            var reviewResponse = ((Response<PowerReviewsResponse>)reviews.Execute(reviewsRequest)).resultset;
            return new Tuple<List<ReviewItem>, Pagination>(reviewResponse.Reviews, reviewResponse.Pagination);
        }

        private FaceOff GetPwFaceOff(string productId)
        {
            var faceOffResponse = new PowerReviewFaceOffResponse();
            var faceOffRequest = new PowerReviewFaceoffRequest();
            var faceoff = new PowerReviewGetFaceOff(_core);
            faceOffRequest.ProductId = productId;
            return ((Response<PowerReviewFaceOffResponse>)faceoff.Execute(faceOffRequest)).resultset.FaceOff;
        }

        private Dictionary<string, ProductDetailItemExtension> GetDetailExtension(string segment = "")
        {
            var productDetailExtension = new ProductDetailExtension(_core, _errors);
            var productDetailExtensionRequest = new ProductDetailExtensionRequest
            {
                VariantIds = segment
            };
            var productDetailExtensionResponse = productDetailExtension.Execute(productDetailExtensionRequest);
            var variantIdToExtension = ((Response<ProductDetailExtensionResponse>) productDetailExtensionResponse).resultset.VariantIdToExtension;
            return variantIdToExtension;
        }
    }
}
