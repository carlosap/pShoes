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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutPayPalCancel", RequestType = typeof(CheckoutPayPalCancelRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CheckoutPayPalCancel : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CheckoutPayPalCancel(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public CheckoutPayPalCancel(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutPayPalCancel()
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
                var request = (CheckoutPayPalCancelRequest) parameters;

                var session = new PaylessSession(_core);

                var checkout = session.GetCheckout();

                var url = string.Format("{0}{1}?token={2}",
                    Config.Urls.SecureBaseUrl, checkout.PayPalInfo.IsBillingInfoStep ? Config.Urls.CheckoutPayPalCancelBilling : Config.Urls.CheckoutPayPalCancelCart, 
                        request.Token);

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutPayPalCancel.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutPayPalCancel.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
