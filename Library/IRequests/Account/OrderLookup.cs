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


namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "OrderLookup", RequestType = typeof(OrderLookupRequest), ResponseType = typeof(Response<AccountOrderDetailResponse>))]
    public class OrderLookup : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        private OrderLookupRequest _request;

        public OrderLookup(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public OrderLookup()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            IResponseBase result;
            var orderLookupForm = new OrderLookupForm(_core, _errors);
            var orderLookupFormRequest = new EmptyRequest();
            var orderLookupFormResponse = (Response<StringResponse>)orderLookupForm.Execute(orderLookupFormRequest);

            if (!_errors.Any())
            {
                _request = (OrderLookupRequest)parameters;
                _request.QueryString = orderLookupFormResponse.resultset.Model;

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
                var url = string.Format("{0}{1}{2}", Config.Urls.SecureBaseUrl, Config.Urls.OrderLookup, _request.QueryString);
                var postSb = new StringBuilder();

                postSb.AppendFormat("dwfrm_ordertrack_orderNumber={0}", HttpUtility.UrlEncode(_request.OrderId));
                postSb.AppendFormat("&dwfrm_ordertrack_emailAddress={0}", HttpUtility.UrlEncode(_request.Email));
                postSb.Append("&dwfrm_ordertrack_findorder=Check+Status");

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString(),
                    OptionalBlockRemoveBlockLinefeedsAndTabs = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("OrderLookup.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("OrderLookup.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
