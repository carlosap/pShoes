using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web;
using Enums;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using MadServ.Core.Models.Responses.PrimitiveResponses;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ResetPassword", RequestType = typeof(ResetPasswordRequest), ResponseType = typeof(Response<BoolResponse>))]
    public class ResetPassword : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        private ResetPasswordRequest _request;
        private PaylessSession _session;

        public ResetPassword(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
            _session = new PaylessSession(core);
        }
        public ResetPassword()
        {
            _errors = new List<SiteError>();
           

        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            IResponseBase result = new Response<BoolResponse>();

            try
            {
                _request = (ResetPasswordRequest)parameters;

                var ResetPasswordForm = new ResetPasswordForm(_core, _errors);
                var ResetPasswordFormRequest = new ResetPasswordFormRequest { Token = _request.Token };
                var ResetPasswordFormResponse = (Response<StringResponse>)ResetPasswordForm.Execute(ResetPasswordFormRequest);

                if (!_errors.Any())
                {
                    _request.DWQuery = ResetPasswordFormResponse.resultset.Model;

                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ProcessResponse(_response, parameters);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ResetPassword.Execute", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.ResetPassword, _request.DWQuery);
                var postSb = new StringBuilder();
                postSb.AppendFormat("dwfrm_resetpassword_password={0}", HttpUtility.UrlEncode(_request.Password));
                postSb.AppendFormat("&dwfrm_resetpassword_passwordconfirm={0}", HttpUtility.UrlEncode(_request.ConfirmPassword));
                postSb.Append("&dwfrm_resetpassword_send=Apply");
                if (_session == null)
                {
                    _session = new PaylessSession(_core);
                }
                var checkout = _session.GetCheckout();
                if (checkout != null)
                {
                    if (!string.IsNullOrEmpty(checkout.CsrfToken))
                    {
                        postSb.AppendFormat("&csrf_token={0}", checkout.CsrfToken);
                    }
                }
                postSb.AppendFormat("&Token={0}", _request.Token);
                
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString(),
                    OverrideGetTemplate = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ResetPassword.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("ResetPassword.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<BoolResponse>();
            try
            {
                var xDoc = response.XDocument;
                var ns = xDoc.Root.GetDefaultNamespace();
                result.resultset.Model = response.RawData.ToLower().Contains("password changed");
                if (!result.resultset.Model)
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.ResetPasswordError, Config.Constants.ResetPasswordError),
                        Severity = ErrorSeverity.UserActionRequired,
                        Type = ErrorType.UserActionRequired
                    });
                }
            }
            catch(Exception ex)
            {
                _errors.Add(ex.Handle("ResetPassword.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }
    }
}
