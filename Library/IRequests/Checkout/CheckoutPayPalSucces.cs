using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutPayPalSuccess", RequestType = typeof(CheckoutPayPalSuccessRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CheckoutPayPalSuccess : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public CheckoutPayPalSuccess()
        {
            _errors = new List<SiteError>();
        }
        public CheckoutPayPalSuccess(ICore core)
            : this()
        {
            _core = core;
        }
        public CheckoutPayPalSuccess(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }

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
                var request = (CheckoutPayPalSuccessRequest)parameters;

                var session = new PaylessSession(_core);

                var cart = session.GetCheckout();

                var url = new StringBuilder();

                url.Append(Config.Urls.SecureBaseUrl);
                url.Append(Config.Urls.CheckoutPayPalSuccess);

                url.AppendFormat("?dwcont={0}", request.DWControl);
                url.AppendFormat("&token={0}", request.Token);
                url.AppendFormat("&PayerID={0}", request.PayerId);

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url.ToString(), _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutPayPalSuccess.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutPayPalSuccess.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
