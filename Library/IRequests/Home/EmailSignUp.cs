using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Enums;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses.PrimitiveResponses;
namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "EmailSignUp",
        RequestType = typeof(EmailSignUpRequest), ResponseType = typeof(Response<BoolResponse>))]
    public class EmailSignUp : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<BoolResponse>();
            var request = (EmailSignUpRequest) parameters;
            var stepTwoUrl = ExecuteStep1(request);
            var stepThreeUrl = ExecuteStep2(stepTwoUrl, request);
            result.resultset.Model = ExecuteStep3(stepThreeUrl, request);
            return result;
        }

        private string ExecuteStep1(EmailSignUpRequest request)
        {
            var comRequest = BuildUrlStep1(request);
            var response = _core.RequestManager.Communicate(comRequest);
            var nextStepUrl = ParseFormAction(response.XDocument, "dwfrm_emailconfirm");

            return nextStepUrl;
        }

        private ICommunicationRequest BuildUrlStep1(IRequestParameter parameters)
        {
            var url = Config.Urls.BaseUrl + Config.Urls.EmailSignupStep1;
            try
            {
                var request = (EmailSignUpRequest) parameters;
                if (request != null)
                    url += "&email=" + request.Email;

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("EmailSignup.BuildUrl", ErrorSeverity.Critical, ErrorType.BuildUrl));
            }
            return _core.CommunicationRequest;
        }

        private string ExecuteStep2(string url, EmailSignUpRequest request)
        {
            var comRequest = BuildUrlStep2(url, request);
            var response = _core.RequestManager.Communicate(comRequest);
            var nextStepUrl = ParseFormAction(response.XDocument, "dwfrm_emailinfo");

            return nextStepUrl;
        }

        private ICommunicationRequest BuildUrlStep2(string url, EmailSignUpRequest request)
        {
            try
            {
                if (request != null)
                {
                    url += "?firstemail=" + request.Email;
                    url += "&confirmemail=" + request.Email;
                    url += "&format=ajax";
                }

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("EmailSignup.BuildUrl", ErrorSeverity.Critical, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        private ICommunicationRequest BuildUrlStep3(string url, EmailSignUpRequest request)
        {
            try
            {
                if (request != null)
                {
                    url += "?email=" + request.Email;
                    url += "&firstemail=" + request.Email;
                    url += "&firstname=" + request.FirstName;
                    url += "&lastname=" + request.LastName;
                    url += "&gender=" + (request.IsFemale ? "1" : "0");
                    url += "&state=" + request.State;
                    url += "&zip=" + request.Zip;
                    url += "&birthmonth=" + request.BirthMonth;
                    url += "&birthday=" + request.BirthDay;
                    url += "&format=ajax";
                }

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("EmailSignup.BuildUrl", ErrorSeverity.Critical, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        private bool ExecuteStep3(string url, EmailSignUpRequest request)
        {
            var comRequest = BuildUrlStep3(url, request);
            var response = _core.RequestManager.Communicate(comRequest);
            var success = ParseSuccess(response.XDocument);
            return success;
        }

        private string ParseFormAction(XDocument xDoc, string formID)
        {
            var result = "";
            var form = xDoc.Descendants("form").WhereAttributeEquals("id", formID).FirstOrDefault();
            if (form == null) return result;
            result = form.AttributeValue("action");
            return result;
        }

        private bool ParseSuccess(XDocument xDoc)
        {
            var result = false;
            var div = xDoc.Descendants("div").WhereAttributeContains("class", "email-modal-complete").FirstOrDefault();
            if (div == null) return result;
            var h2 = div.Descendants("h2").FirstOrDefault();

            if (h2 == null) return result;
            result = h2.Value.ToUpper().Contains("CONGRATULATIONS");

            return result;
        }
    }
}
