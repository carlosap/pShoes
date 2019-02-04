using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Enums;
using Library.Helpers;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;


namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ForgotPasswordForm",
        RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<ForgotPasswordFormResponse>))]
    public class ForgotPasswordForm : IRequest
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
            var url = Config.Urls.SecureBaseUrl + Config.Urls.ForgotPasswordForm;

            var checkout = _session.GetCheckout();
            if (checkout != null)
            {               
                if (!string.IsNullOrEmpty(checkout.CsrfToken))
                {
                    url = string.Format("{0}{1}&csrf_token={2}", Config.Urls.SecureBaseUrl, Config.Urls.ForgotPasswordForm,checkout.CsrfToken);
                }
            }

            try
            {
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ForgotPasswordForm.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("ForgotPasswordForm.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<ForgotPasswordFormResponse>();

            try
            {
                var xDoc = response.XDocument;
                var ns = xDoc.Root.GetDefaultNamespace();

                var form = xDoc.Descendants(ns + "form")
                    .WhereAttributeEquals("id", "PasswordResetForm")
                    .FirstOrNewXElement();

                var action = form.AttributeValue("action");
                var dwSecureKey = form.Descendants(ns + "input")
                    .WhereAttributeEquals("name", "dwfrm_requestpassword_securekey")
                    .FirstOrNewXElement()
                    .AttributeValue("value");

                if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(dwSecureKey))
                {
                    result.resultset.Action = action;
                    result.resultset.DWSecureKey = dwSecureKey;
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.GenericError, Config.Constants.GenericError),
                        Severity = ErrorSeverity.FollowUp,
                        Type = ErrorType.Unclassifiable
                    });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ForgotPasswordForm.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }

        #region constructor and parameters

        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PaylessSession _session;

        public ForgotPasswordForm(ICore core, List<SiteError> errors)
        {
            _errors = errors;
            _core = core;
            _session = new PaylessSession(core);
        }

        public ForgotPasswordForm(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
            _session = new PaylessSession(core);
        }

        public ForgotPasswordForm()
        {
            _errors = new List<SiteError>();
        }

        #endregion
    }
}