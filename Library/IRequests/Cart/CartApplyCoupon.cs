using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Enums;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using Newtonsoft.Json;
using Library.Helpers;
using Newtonsoft.Json.Linq;


namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CartApplyCoupon", RequestType = typeof(CartApplyCouponRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CartApplyCoupon : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CartApplyCoupon(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CartApplyCoupon()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);

            //4/23/2015, JAY: they added a new McAfee symbol which contained malformed html
            //CustomStreamReaderProcess removes the junk; commenting this out for now because they supposedly fixed it.
            //communicationRequest.OptionalStreamReaderProcess = ParsingHelper.CustomStreamReaderProcess;

            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);
            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = (CartApplyCouponRequest)parameters;
                var cart = (new PaylessSession(_core)).GetCheckout().Cart;
                string url = string.Empty;
                var postSb = new StringBuilder();


                url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CartApplyCoupon, cart.DWQuery);
                postSb.Append("dwfrm_cart_updateCart=dwfrm_cart_updateCart");
                postSb.Append("&dwfrm_cart_addCoupon=dwfrm_cart_addCoupon");
                postSb.AppendFormat("&dwfrm_cart_couponCode={0}", request.Coupons.FirstOrDefault().Code);

                //cperez: SPAY-46 and SPAY-47 - 11/10/2015
                //targets new dependencies with reward coupons.
                if (!string.IsNullOrWhiteSpace(request.Email))
                    postSb.AppendFormat("&dwfrm_cart_rewardsEmail={0}", request.Email);

                for (int i = 0; i < cart.CartItems.Count; ++i)
                {
                    var item = cart.CartItems.ElementAt(i);
                    postSb.AppendFormat("&dwfrm_cart_shipments_i0_items_i{0}_quantity={1}", i, item.Quantity);
                }

                //cpere: 11/25/2015- this is hack due to multiple env.
                //the node version has been rewritten to support single point URL
                //the code below, ensures the querystring is not use for the new url format
                if (url.ToUpper().Contains("SITES-PAYLESS"))
                {
                    url = url.Replace("?dwcont=", "/");
                }
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString()
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CartApplyCoupon.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        public ICommunicationRequest BuildUrlRedirect(string url)
        {
            try
            {
                var cart = (new PaylessSession(_core)).GetCheckout().Cart;
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    //OverridePostData = postSb.ToString()
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CartApplyCoupon.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CartApplyCoupon.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }


        /// <summary>
        ///  SPAY-46 and SPAY-47
        ///  Date: 11/10/2015
        ///  cperez: ProcessCart added extension to the 
        ///  result object. "Coupon" is now aviable for 
        ///  client UI to parse and add new behaviors.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
           
            Response<CartResponse> results = new Response<CartResponse>();
            results = (Response<CartResponse>)response.Template.Service.Process(response, parameters, _errors);
            var request = (CartApplyCouponRequest)parameters;
            Coupon coupon = GetLoyalCoupon(request, parameters);
            results.resultset.CouponStatus = coupon;
            return results;
        }
 
        /// <summary>
        /// SPAY-46 and SPAY-47
        /// Date: 11/10/2015
        /// cperez: GetLoyalCoupon makes a subrequest
        /// to detect if the coupon entered is loyal coupon
        /// this workaround was needed due to the new URLs desktop 
        /// is providing. Loyal coupons don't work on old legacy URLs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Coupon GetLoyalCoupon(CartApplyCouponRequest request, IRequestParameter parameters)
        {
            Coupon coupon = new Coupon();
            try
            {
                
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    coupon = RetryCouponBehaviorAtOnDemandWare(parameters);
                }
            }
            catch (Exception)
            {
                coupon.couponType = "";
                coupon.CouponValue = null;
                coupon.loyaltyRequired = "false";
                coupon.message = "";
            }

            return coupon;
        }
 
        /// <summary>
        /// SPAY-46 and SPAY-47
        /// Date: 11/10/2015
        /// cperez: RetryCouponBehaviorAtOnDemandWare
        /// parses the json frombody part of the response
        /// see GetLoyalCoupon...
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Coupon RetryCouponBehaviorAtOnDemandWare(IRequestParameter parameters)
        {
            Coupon coupon = new Coupon();
            string results = string.Empty;
            try
            {
                //https://staging.payless.com/on/demandware.store/Sites-payless-Site/default/COSinglePage-Start
                var request = (CartApplyCouponRequest)parameters;
                string hostEnvPath = Config.Urls.CartOnDemandWareAddCoupon;
                string hostEnv = Config.Urls.Domain;
                string couponCode = request.Coupons.FirstOrDefault().Code;
                string email = request.Email;
                string url = string.Format("https://{0}{1}?couponCode={2}&format=ajax&couponLoyaltyEmail={3}", hostEnv, hostEnvPath, couponCode, email);
                results = CartApplyCouponOnDemandWare.MakeRequest(url);
                
            }
            catch (Exception ex)
            {
                coupon.couponType = "";
                coupon.CouponValue = null;
                coupon.loyaltyRequired = "false";
                coupon.message = "Error: RetryCouponBehaviorAtOnDemandWare- "+ ex.Message;
            }
            
            var couponObj = JsonConvert.DeserializeObject(results);
            coupon.status = ((dynamic)((JObject)(couponObj))).status;
            coupon.loyaltyRequired = ((dynamic)((JObject)(couponObj))).loyaltyRequired;
            coupon.message = ((dynamic)((JObject)(couponObj))).message.ToString();
            coupon.sucess = ((dynamic)((JObject)(couponObj))).success;
            coupon.couponType = ((dynamic)((JObject)(couponObj))).couponType.ToString();
            return coupon;
        }
    }
}
