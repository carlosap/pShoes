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
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using MadServ.Core.Models.Responses.PrimitiveResponses;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ForgotPassword", RequestType = typeof(ForgotPasswordRequest), ResponseType = typeof(Response<BoolResponse>))]
    public class ForgotPassword : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        private ForgotPasswordRequest _request;

        public ForgotPassword(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public ForgotPassword()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            IResponseBase result;
            var ForgotPasswordForm = new ForgotPasswordForm(_core, _errors);
            var ForgotPasswordFormRequest = new EmptyRequest();
            var ForgotPasswordFormResponse = (Response<ForgotPasswordFormResponse>)ForgotPasswordForm.Execute(ForgotPasswordFormRequest);

            if (!_errors.Any())
            {
                _request = (ForgotPasswordRequest)parameters;
                _request.Form = ForgotPasswordFormResponse.resultset;

                var communicationRequest = BuildUrl(parameters);
                _response = Communicate(communicationRequest);
                result = ProcessResponse(_response, parameters);
            }
            else
            {
                result = new Response<AccountOrderDetailResponse>();
            }
            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var querySb = new StringBuilder();

                querySb.Append("format=ajax");
                querySb.AppendFormat("&dwfrm_requestpassword_email={0}", HttpUtility.UrlEncode(_request.UserName));
                querySb.AppendFormat("&dwfrm_requestpassword_securekey={0}", _request.Form.DWSecureKey);
                querySb.Append("&dwfrm_requestpassword_send=submit");

                var url = string.Format("{0}?{1}", _request.Form.Action, querySb.ToString());

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ForgotPassword.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("ForgotPassword.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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

                result.resultset.Model = xDoc.Descendants("h1")
                                             .FirstOrNewXElement()
                                             .ElementValue()
                                             .ToLowerInvariant()
                                             .Contains("reset your password received");

                if (!result.resultset.Model)
                {
                    var error = xDoc.Descendants(ns + "div")
                                     .WhereAttributeEquals("class", "error-form")
                                     .FirstOrNewXElement()
                                     .ElementValue();

                    if (!string.IsNullOrEmpty(error))
                    {
                        _errors.Add(new SiteError
                        {
                            Message = new ErrorMessage(error, error),
                            Severity = ErrorSeverity.UserActionRequired,
                            Type = ErrorType.UserActionRequired
                        });
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
            }
            catch(Exception ex)
            {
                _errors.Add(ex.Handle("ForgotPassword.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }
    }
}
