using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Enums;
using Library.Models.Requests;
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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "Logout", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<BoolResponse>))]
    public class Logout : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public Logout(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public Logout()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);
            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            string url = Config.Urls.SecureBaseUrl + Config.Urls.Logout;

            try
            {
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true,
                    OverrideBlockXDocumentConversion = true,
                    OptionalReturnResponseHeaders = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Logout.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("Logout.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<BoolResponse>();

            if (response.ResponseHeaders.ContainsKey("ResponseUri") && response.ResponseHeaders["ResponseUri"].EndsWith("/account"))
            {
                response.Template = new Template { TemplateName = Config.TemplateEnum.Logout };
                result.resultset.Model = true;

                (new PaylessSession(_core)).RemoveCheckout();
            }

            return result;
        }
    }
}
