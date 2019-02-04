using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Enums;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutReview", RequestType = typeof(CheckoutReviewRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutReview : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CheckoutReview(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutReview()
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
                var checkout = ((CheckoutReviewRequest)parameters).CheckoutResponse;

                if (checkout != null)
                {
                    var url = string.Format("{0}{1}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutReviewPost);
                    if (!string.IsNullOrEmpty(checkout.CsrfToken))
                    {
                        url = url + "?csrf_token=" + checkout.CsrfToken;
                    }
                    var postSb = new StringBuilder();

                    postSb.Append("submit=Place+Your+Order");
                    _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                    {
                        OverridePostData = postSb.ToString()
                    };
                }
                else
                {
                    var url = string.Format("{0}{1}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutReviewGet);
                    _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutReview.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutReview.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
