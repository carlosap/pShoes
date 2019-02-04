using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Enums;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using Newtonsoft.Json;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutApplyCoupon", RequestType = typeof(CheckoutApplyCouponRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutApplyCoupon : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private CheckoutApplyCouponRequest _request;

        public CheckoutApplyCoupon(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutApplyCoupon()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);
            _response = Communicate(communicationRequest);
            var result = ProcessResponse(_response, parameters);

            if (!_errors.Any())
            {
                var checkoutUpdateSummaryRequest = new CheckoutUpdateSummaryRequest
                {
                    CheckoutResponse = ((Response<CheckoutResponse>)result).resultset
                };
                var checkoutUpdateSummary = new CheckoutUpdateSummary(_core, _errors);
                result = checkoutUpdateSummary.Execute(checkoutUpdateSummaryRequest);
            }

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                _request = (CheckoutApplyCouponRequest) parameters;
                var url = string.Format("{0}{1}?couponCode={2}&format=ajax", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutApplyCoupon, _request.CouponCode);
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true,
                    OverrideBlockXDocumentConversion = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutApplyCoupon.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutApplyCoupon.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>();
            result.resultset =  _request.CheckoutResponse;

            try
            {
                var dto = JsonConvert.DeserializeObject<CheckoutApplyCouponDTO>(response.RawData);

                if (dto.Success)
                {
                    result.resultset.ApplyCouponMessage = dto.Message;

                    (new PaylessSession(_core)).SetCheckout(result.resultset);
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(dto.Message, dto.Message),
                        Severity = ErrorSeverity.UserActionRequired,
                        Type = ErrorType.UserActionRequired
                    });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutApplyCoupon.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return result;
        }
    }
}
