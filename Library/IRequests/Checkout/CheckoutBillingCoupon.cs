using Enums;
using Library.DemandWare.Models;
using Library.DemandWare.RequestManager;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using Library.Services;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutBillingCoupon", RequestType = typeof(CartApplyCouponRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutBillingCoupon : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CheckoutBillingCoupon(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutBillingCoupon()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            IResponseBase result = new Response<CheckoutResponse>();
            var config = BuildAPI(parameters);
            result = GetResponse(config, parameters);

            return result;
        }


        private DWClientConfig BuildAPI(IRequestParameter parameters)
        {
            var theParams = (CartApplyCouponRequest)parameters;
            var path = string.Format("{0}?{1}", Config.Urls.ApplyBillingCouponPart1, "couponCode=" + theParams.Coupons.FirstOrDefault().Code + "&format=ajax");
            var sb = new StringBuilder();

            return new DWClientConfig(_core)
            {
                Path = path,
                Query = sb.ToString(),
                Method = HttpMethod.GET
            };
        }


        private IResponseBase GetResponse(DWClientConfig config, IRequestParameter parameters)
        {
            var theParams = (CartApplyCouponRequest)parameters;
            var result = new Response<CheckoutResponse>();
            try
            {
                var apiResponse = DWClient.GetAPIResponse<CouponResponse>(config, true);

                if (apiResponse.ResponseStatus.Code == HttpStatusCode.OK)
                {
                    if (apiResponse.Model.success)
                    {
                        _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, Config.Urls.ApplyBillingCouponPart2, _core, _errors);
                        var response = _core.RequestManager.Communicate(_core.CommunicationRequest);                 
                        result = ProcessCart(response, parameters);
                        result.resultset.Cart.Coupons.Add(new CouponCodeBase() { Code = theParams.Coupons[0].Code, Message = apiResponse.Model.message });
                    }
                    else
                    {
                        _errors.Add(new SiteError { Message = new ErrorMessage(apiResponse.Model.message,Config.Constants.GenericError) });
                    }
                    
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBillingCoupon.GetResponse", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        public Response<CheckoutResponse> ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            CheckoutService s = new CheckoutService(_core);
            return s.ProcessCoupon(response, parameters);
        }
    }
}
