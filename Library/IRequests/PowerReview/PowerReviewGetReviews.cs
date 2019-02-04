using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Caching;
using Enums;
using Library.Cache;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.PowerReview;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.RequestHandler;
using MadServ.Core.Models.Responses;
using Library.Models.Responses;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "PowerReviewGetReviews", RequestType = typeof(PowerReviewsRequest), ResponseType = typeof(Response<PowerReviewsResponse>))]
    public class PowerReviewGetReviews : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PowerReviewsRequest _request;
        private int _displayFrom;
        private int _displayTo;
        private int _pageSize;
        private int _currentPage;
        private string _sort;
        public PowerReviewGetReviews(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public PowerReviewGetReviews(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public PowerReviewGetReviews()
        {
            _errors = new List<SiteError>();
        }
        public IResponseBase Execute(IRequestParameter parameters)
        {            
            var result = new Response<PowerReviewsResponse>();
            try
            {
                _request = (PowerReviewsRequest)parameters;
                _request.Sort = (!string.IsNullOrEmpty(_request.Sort)) ? _request.Sort : "created_date desc";
                var cacheKey = string.Format(Config.PowerReview.CacheKeyReviews, _request.ProductId, _request.PageNumber,_request.Sort);
                result = CacheMemory.Get<Response<PowerReviewsResponse>>(cacheKey);
                if (result == null || result.resultset.Reviews.Count == 0)
                {
                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ParseResponse(_response);
                    if (result.resultset.Reviews.Count > 0)
                    {
                        //per lead's request. only cache the first page
                        if (_request.PageNumber == 0)
                        {
                            CacheMemory.SetAndExpiresMinutesAsync(cacheKey, result, Config.PowerReview.Cache_TTL_InMinutes);
                        }                      
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetReviews.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }
        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = parameters as PowerReviewsRequest;
                var productId = request.ProductId;
                var page = request.PageNumber;
                _sort = request.Sort;
                _pageSize = Config.PowerReview.PageSize;
                var url = string.Format(Config.PowerReview.ReviewsUrl,Config.PowerReview.ApiKey,Config.PowerReview.MerchantId,_pageSize,request.ProductId);
                if (page == 0)
                {
                    _displayFrom = 1;
                    _displayTo = _pageSize;
                }
                else
                {
                    _displayFrom = (page * _pageSize) + 1;
                    _displayTo = (page * _pageSize) + _pageSize;                 
                }
                _currentPage = page;
                url = url + "&page=" + page;
                _sort = (!string.IsNullOrEmpty(_sort)) ? _sort : "created_date desc";
                url = url + "&sort=" + _sort;
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetReviews.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return _core.CommunicationRequest;
        }
        public IResultResponse Communicate(ICommunicationRequest request)
        {
            try
            {
                var resultResponse = _core.RequestManager.Communicate(request);
                return resultResponse;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetReviews.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return new ResultResponse(); 
        }
        public Response<PowerReviewsResponse> ParseResponse(IResultResponse response)
        {
            var result = new Response<PowerReviewsResponse>();
            result.resultset.Reviews = PowerReviewHelper.GetReviews(response.RawData);
            result.resultset.Pagination.DisplayFrom = _displayFrom;
            result.resultset.Pagination.DisplayTo = _displayTo;
            result.resultset.Pagination.CurrentPage = _currentPage;
            result.resultset.Pagination.Sort = _sort;
            return result;
        }
    }
}



