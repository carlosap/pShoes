using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web;
using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using MadServ.Core.Models.Responses.PrimitiveResponses;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "Login", RequestType = typeof(LoginRequest), ResponseType = typeof(Response<BoolResponse>))]
    public class Login : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        private LoginRequest _request;

        public Login(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public Login()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            IResponseBase result = new Response<BoolResponse>();

            try
            {
                _request = (LoginRequest)parameters;

                var LoginForm = new LoginForm(_core, _errors);
                var LoginFormRequest = new EmptyRequest();
                var LoginFormResponse = (Response<LoginFormResponse>)LoginForm.Execute(LoginFormRequest);

                if (!_errors.Any())
                {
                    _request.Form = LoginFormResponse.resultset;

                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ProcessResponse(_response, parameters);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Login.Execute", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var url = _request.Form.Action;
                var postSb = new StringBuilder();

                postSb.AppendFormat("{0}={1}", HttpUtility.UrlEncode(_request.Form.DWLoginParam), HttpUtility.UrlEncode(_request.UserName));
                postSb.AppendFormat("&dwfrm_login_password={0}", HttpUtility.UrlEncode(_request.Password));
                postSb.Append("&dwfrm_login_login=Login");
                postSb.AppendFormat("&dwfrm_login_securekey={0}", _request.Form.DWSecureKey);

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString()
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Login.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("Login.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<BoolResponse>();

            try
            {
                if (response.Template.TemplateName.Equals(Config.TemplateEnum.AccountDashboard))
                {
                    result.resultset.Model = true;

                    var session = new PaylessSession(_core);

                    var checkout = session.GetCheckout();
                    checkout.IsLoggedIn = true;

                    session.SetCheckout(checkout);
                }
                else
                {
                    // simply parse errors
                    response.Template.Service.Process(response, parameters, _errors);
                }
            }
            catch(Exception ex)
            {
                _errors.Add(ex.Handle("Login.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }
    }
}
