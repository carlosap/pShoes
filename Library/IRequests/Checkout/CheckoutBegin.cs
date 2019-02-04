using Enums;
using Library.Helpers;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutBegin", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CheckoutBegin : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public CheckoutBegin()
        {
            _errors = new List<SiteError>();
        }
        public CheckoutBegin(ICore core)
            : this()
        {
            _core = core;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl();
            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);
            return result;
        }

        public ICommunicationRequest BuildUrl()
        {
            try
            {
                var cart = (new PaylessSession(_core)).GetCheckout().Cart;
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutBegin, cart.DWQuery);

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = "dwfrm_cart_checkoutCart=Checkout"
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBegin.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        public IResultResponse Communicate(ICommunicationRequest request)
        {
            try
            {
                request.OptionalRemoveScriptTags = false;
                var resultResponse = _core.RequestManager.Communicate(request);
                return resultResponse;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBegin.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
