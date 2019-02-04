using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CartCouponRemove", RequestType = typeof(CartRemoveRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CartCouponRemove : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CartCouponRemove(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CartCouponRemove()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);


            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = (CartRemoveRequest)parameters;
                var postSb = new StringBuilder();
                var cart = (new PaylessSession(_core)).GetCheckout().Cart;
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CartRemove, cart.DWQuery);

                if (cart.Coupons.Count() > 0)
                {
                    for (int i = 0; i < cart.Coupons.Count; i++)
                    {
                        postSb.Append("&dwfrm_cart_shipments_i0_items_i0_quantity=1");
                        postSb.Append("&dwfrm_cart_updateCart=dwfrm_cart_updateCart");
                        postSb.AppendFormat("&dwfrm_cart_coupons_i{0}_deleteCoupon", i);
                        postSb.Append("&dwfrm_cart_couponCode=");
                        _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                        {
                            OverridePostData = postSb.ToString()
                        };
                    }
                }
                //---------------PUT ME BACK IF YOU WANT ONLY ONE COUPON TO BE REMOVED-------------------
                //cart.Coupons.RemoveAt(request.ItemIndex);

                //postSb.Append("&dwfrm_cart_shipments_i0_items_i0_quantity=1");
                //postSb.Append("&dwfrm_cart_updateCart=dwfrm_cart_updateCart");
                //postSb.AppendFormat("&dwfrm_cart_coupons_i{0}_deleteCoupon",request.ItemIndex);
                //postSb.Append("&dwfrm_cart_couponCode=");              
                //_core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                //{
                //    OverridePostData = postSb.ToString()
                //};
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CartRemove.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CartRemove.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
