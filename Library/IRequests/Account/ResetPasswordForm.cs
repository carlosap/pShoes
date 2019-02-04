using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Enums;
using Library.Helpers;
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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ResetPasswordForm",
        RequestType = typeof(ResetPasswordFormRequest), ResponseType = typeof(Response<StringResponse>))]
    public class ResetPasswordForm : IRequest
    {
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
                var request = (ResetPasswordFormRequest) parameters;
                var url = string.Format("{0}{1}?Token={2}", Config.Urls.SecureBaseUrl, Config.Urls.ResetPasswordForm,request.Token);

                var checkout = _session.GetCheckout();
                if (checkout != null)
                {
                    if (!string.IsNullOrEmpty(checkout.CsrfToken))
                    {
                        url = string.Format("{0}&csrf_token={1}",url, checkout.CsrfToken);
                    }
                }

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ResetPasswordForm.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("ResetPasswordForm.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<StringResponse>();
            var xDoc = response.XDocument;

            try
            {
                result.resultset.Model = ParsingHelper.GetDWQuery(xDoc, "NewPasswordForm");
                if (string.IsNullOrEmpty(result.resultset.Model))
                {
                    result.resultset.Model = ParsingHelper.GetDWQuery(xDoc, "PasswordResetForm");
                    if(string.IsNullOrEmpty(result.resultset.Model))
                    {
                       _errors.Add(new SiteError
                       {
                           Message = new ErrorMessage(Config.Constants.GenericError, Config.Constants.GenericError),
                           Severity = ErrorSeverity.UserActionRequired,
                           Type = ErrorType.UserActionRequired
                       });
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ResetPasswordForm.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }

        #region constructor and parameters

        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PaylessSession _session;
        public ResetPasswordForm(ICore core, List<SiteError> errors)
        {
            _errors = errors;
            _core = core;
            _session = new PaylessSession(core);
        }

        public ResetPasswordForm(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
            _session = new PaylessSession(core);
        }

        public ResetPasswordForm()
        {
            _errors = new List<SiteError>();
        }

        #endregion
    }
}