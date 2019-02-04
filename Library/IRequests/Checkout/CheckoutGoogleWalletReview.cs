using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Enums;
using Library.Helpers;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutGoogleWalletReview", RequestType = typeof(CheckoutGoogleWalletReviewRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CheckoutGoogleWalletReview : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CheckoutGoogleWalletReview(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public CheckoutGoogleWalletReview(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutGoogleWalletReview()
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
                var request = (CheckoutGoogleWalletReviewRequest)parameters;
                var url = Config.Urls.SecureBaseUrl + Config.Urls.CheckoutGoogleWalletReview;
                var postSb = new StringBuilder();

                postSb.AppendFormat("EnvironmentID={0}", EnvironmentHelper.GetEnvironmentId(_core.Context.Request));
                postSb.AppendFormat("&dwfrm_googlewallet_mwResponse={0}", request.Jwt);

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString()
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutGoogleWalletReview.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutGoogleWalletReview.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
