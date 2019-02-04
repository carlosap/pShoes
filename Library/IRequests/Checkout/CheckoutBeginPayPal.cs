using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Enums;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using MadServ.Core.Models.Responses.PrimitiveResponses;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutBeginPayPal", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutBeginPayPal : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CheckoutBeginPayPal(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutBeginPayPal()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);
            _response = Communicate(communicationRequest);
            var result = ProcessResponse(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var cart = (new PaylessSession(_core)).GetCheckout().Cart;
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutBeginPayPal,
                    cart.DWQuery);
                var postSb = new StringBuilder();

                postSb.Append("dwfrm_cart_expressCheckout=");
                postSb.AppendFormat("&EnvironmentID={0}", EnvironmentHelper.GetEnvironmentId(_core.Context.Request));

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString(),
                    OverrideStopAutoRedirects = true,
                    OverrideBlockXDocumentConversion = true,
                    OptionalReturnResponseHeaders = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBeginPayPal.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutBeginPayPal.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            IResponseBase result = new Response<BoolResponse>();

            try
            {
                if (response.Template.Service != null)
                {
                    result = response.Template.Service.Process(response, parameters, _errors);
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.GenericError, Config.Constants.GenericError),
                        Severity = ErrorSeverity.UserActionRequired,
                        Type = ErrorType.UserActionRequired
                    });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBeginPayPal.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;

        }
    }
}
