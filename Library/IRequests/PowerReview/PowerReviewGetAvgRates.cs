using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.PowerReview;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.RequestHandler;
using MadServ.Core.Models.Responses;
using Library.Cache;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "PowerReviewGetAvgRates", RequestType = typeof(PowerReviewAvgRateRequest), ResponseType = typeof(Response<PowerReviewAvgRateResponse>))]
    public class PowerReviewGetAvgRates : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PowerReviewAvgRateRequest _request;
        public PowerReviewGetAvgRates(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public PowerReviewGetAvgRates(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public PowerReviewGetAvgRates()
        {
            _errors = new List<SiteError>();
        }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<PowerReviewAvgRateResponse>();
            try
            {
                _request = (PowerReviewAvgRateRequest)parameters;
                var cacheKey = string.Format(Config.PowerReview.CacheKeyAvgRate, _request.ProductId);
                result = CacheMemory.Get<Response<PowerReviewAvgRateResponse>>(cacheKey);
                if (result == null || result.resultset.AvgRates.Count == 0)
                {
                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ParseResponse(_response);
                    if (result.resultset.AvgRates.Count > 0)
                    {
                        if (Config.PowerReview.IsCacheEnabled)
                        {
                            CacheMemory.SetAndExpiresMinutesAsync(cacheKey, result, Config.PowerReview.Cache_TTL_InMinutes);
                        }      
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetAvgRates.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }
        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = parameters as PowerReviewAvgRateRequest;
                var url = string.Format(Config.PowerReview.RatingUrl,Config.PowerReview.ApiKey,Config.PowerReview.MerchantId,request.ProductId); 
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetAvgRates.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("PowerReviewGetAvgRates.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return new ResultResponse(); 
        }
        public Response<PowerReviewAvgRateResponse> ParseResponse(IResultResponse response)
        {
            var result = new Response<PowerReviewAvgRateResponse>();
            result.resultset.AvgRates = PowerReviewHelper.GetAvgRates(response.RawData);
            return result;
        }
    }
}

