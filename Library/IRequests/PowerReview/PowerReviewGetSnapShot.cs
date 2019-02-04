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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "PowerReviewSnapshot", RequestType = typeof(PowerReviewSnapshotRequest), ResponseType = typeof(Response<PowerReviewSnapshotResponse>))]
    public class PowerReviewGetSnapshot : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PowerReviewSnapshotRequest _request;
        public PowerReviewGetSnapshot(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public PowerReviewGetSnapshot(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public PowerReviewGetSnapshot()
        {
            _errors = new List<SiteError>();
        }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<PowerReviewSnapshotResponse>();
            try
            {
                _request = (PowerReviewSnapshotRequest)parameters;
                var cacheKey = string.Format(Config.PowerReview.CacheKeySnapShot, _request.ProductId);
                result = CacheMemory.Get<Response<PowerReviewSnapshotResponse>>(cacheKey);
                if (result == null || (string.IsNullOrEmpty(result.resultset.Snapshot.average_rating)))
                {
                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ParseResponse(_response);
                    if (!string.IsNullOrEmpty(result.resultset.Snapshot.average_rating))
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
                _errors.Add(ex.Handle("PowerReviewGetSnapshot.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }
        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = parameters as PowerReviewSnapshotRequest;
                var url = string.Format(Config.PowerReview.SnapshotUrl, Config.PowerReview.ApiKey,Config.PowerReview.MerchantId,request.ProductId);
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
                _errors.Add(ex.Handle("PowerReviewSnapshot.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return new ResultResponse(); 
        }
        public Response<PowerReviewSnapshotResponse> ParseResponse(IResultResponse response)
        {
            var result = new Response<PowerReviewSnapshotResponse>();
            result.resultset.Snapshot = PowerReviewHelper.GetSnapshot(response.RawData);
            return result;
        }
    }
}



