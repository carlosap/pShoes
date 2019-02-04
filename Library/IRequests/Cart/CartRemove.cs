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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CartRemove", RequestType = typeof(CartRemoveRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CartRemove : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CartRemove(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CartRemove()
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
                var request = (CartRemoveRequest)parameters;
                var postSb = new StringBuilder();
                var cart = (new PaylessSession(_core)).GetCheckout().Cart;
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CartRemove, cart.DWQuery);

                postSb.Append("dwfrm_cart_updateCart=dwfrm_cart_updateCart");
                postSb.Append("&dwfrm_cart_couponCode=");

                for (int i = 0; i < cart.CartItems.Count; ++i)
                {
                    var item = cart.CartItems.ElementAt(i);
                    postSb.AppendFormat("&dwfrm_cart_shipments_i0_items_i{0}_quantity={1}", i, item.Quantity);
                    if (request.Sku.Equals(item.ProductId))
                    {
                        postSb.AppendFormat("&dwfrm_cart_shipments_i0_items_i{0}_deleteProduct=Remove", i);
                    }
                }

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString()
                };
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
