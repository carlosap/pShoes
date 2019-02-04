using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Web;
using Enums;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CartUser", RequestType = typeof(CartUserRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CartUser : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CartUser(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CartUser()
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
                var checkout = (new PaylessSession(_core)).GetCheckout();
                var request = (CartUserRequest) parameters;
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.CartUser, checkout.Cart.DWQuery);
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
                _errors.Add(ex.Handle("CartUser.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CartUser.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
