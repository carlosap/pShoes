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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "PowerReviewGetFaceOff", RequestType = typeof(PowerReviewFaceoffRequest), ResponseType = typeof(Response<PowerReviewFaceOffResponse>))]
    public class PowerReviewGetFaceOff : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PowerReviewFaceoffRequest _request;
        public PowerReviewGetFaceOff(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public PowerReviewGetFaceOff(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public PowerReviewGetFaceOff()
        {
            _errors = new List<SiteError>();
        }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<PowerReviewFaceOffResponse>();
            try
            {
                _request = (PowerReviewFaceoffRequest)parameters;
                var cacheKey = string.Format(Config.PowerReview.CacheKeyFaceOff, _request.ProductId);
                result = CacheMemory.Get<Response<PowerReviewFaceOffResponse>>(cacheKey);
                if (result == null || result.resultset.FaceOff.positive.rating == 0)
                {
                    _request = (PowerReviewFaceoffRequest)parameters;
                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ParseResponse(_response);
                    if (result.resultset.FaceOff != null)
                    {
                        if (result.resultset.FaceOff.positive.rating > 0)
                        {
                            if (Config.PowerReview.IsCacheEnabled)
                            {
                                CacheMemory.SetAndExpiresMinutesAsync(cacheKey, result, Config.PowerReview.Cache_TTL_InMinutes);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetFaceOff.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }
        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = parameters as PowerReviewFaceoffRequest;
                var url = string.Format(Config.PowerReview.FaceOffUrl,Config.PowerReview.ApiKey,Config.PowerReview.MerchantId,request.ProductId); 
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PowerReviewGetFaceOff.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("PowerReviewGetFaceOff.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return new ResultResponse(); 
        }
        public Response<PowerReviewFaceOffResponse> ParseResponse(IResultResponse response)
        {
            var result = new Response<PowerReviewFaceOffResponse>();
            result.resultset.FaceOff = PowerReviewHelper.GetFaceOffReviews(response.RawData);
            return result;
        }
    }
}



