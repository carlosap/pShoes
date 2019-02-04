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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "OrderLookupForm", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<StringResponse>))]
    public class OrderLookupForm : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public OrderLookupForm(ICore core, List<SiteError> errors)
        {
            _errors = errors;
            _core = core;
        }
        public OrderLookupForm(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public OrderLookupForm()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);
            _response = Communicate(communicationRequest);
            var result = ProcessResponse(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            string url = Config.Urls.SecureBaseUrl + Config.Urls.OrderLookupForm;

            try
            {
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("OrderLookupForm.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("OrderLookupForm.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<StringResponse>();

            try
            {
                var xDoc = response.XDocument;

                result.resultset.Model = ParsingHelper.GetDWQuery(xDoc, "dwfrm_ordertrack");

                if (string.IsNullOrEmpty(result.resultset.Model))
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
                _errors.Add(ex.Handle("OrderLookupForm.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }
    }
}
