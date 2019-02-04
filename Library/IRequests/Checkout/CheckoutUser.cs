using Enums;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Web;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutUser", RequestType = typeof(CheckoutUserRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutUser : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public CheckoutUser()
        {
            _errors = new List<SiteError>();
        }
        public CheckoutUser(ICore core)
            : this()
        {
            _core = core;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var request = (CheckoutUserRequest)parameters;

            var communicationRequest = BuildUrl(request);

            //4/23/2015, JAY: they added a new McAfee symbol which contained malformed html
            //CustomStreamReaderProcess removes the junk; commenting this out for now because they supposedly fixed it.
            //communicationRequest.OptionalStreamReaderProcess = ParsingHelper.CustomStreamReaderProcess;

            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, request);

            return result;
        }

        public ICommunicationRequest BuildUrl(CheckoutUserRequest parameters)
        {
            try
            {
                var session = new PaylessSession(_core);

                var checkout = session.GetCheckout();

                var request = (CheckoutUserRequest) parameters;
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutUser, checkout.Cart.DWQuery);
                var postSb = new StringBuilder();

                postSb.AppendFormat("{0}={1}", checkout.Cart.DWLoginParam, HttpUtility.UrlEncode(request.Email));
                postSb.AppendFormat("&dwfrm_login_password={0}", HttpUtility.UrlEncode(request.Password));
                postSb.Append("&dwfrm_login_login=Login");
                postSb.AppendFormat("&dwfrm_login_securekey={0}", checkout.DWSecureKey);

                if (request.IsRememberMe)
                {
                    postSb.Append("&dwfrm_login_rememberme=true");
                }

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString()
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutUser.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutUser.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, CheckoutUserRequest request)
        {
            PutEmailInSession(request);

            return response.Template.Service.Process(response, request, _errors);
        }

        private void PutEmailInSession(CheckoutUserRequest request)
        {
            //this is why we can't have nice things
            var session = new Session(_core);
            var checkout = session.Get<CheckoutResponse>(Config.Keys.Checkout) ?? new CheckoutResponse();
            checkout.Email = request.Email;
            session.Add<CheckoutResponse>(Config.Keys.Checkout, checkout);
        }
    }
}