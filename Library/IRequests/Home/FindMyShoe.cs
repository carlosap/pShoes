using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "FindMyShoe", RequestType = typeof(FindMyShoeRequest), ResponseType = typeof(Response<FindMyShoeResponse>))]
    public class FindMyShoe : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public FindMyShoe()
        {
            _errors = new List<SiteError>();
        }
        public FindMyShoe(ICore core)
            : this()
        {
            _core = core;
        }
        public FindMyShoe(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl((FindMyShoeRequest)parameters);
            _response = Communicate(communicationRequest);
            var result = Process(_response, parameters);
            return result;
        }

        public ICommunicationRequest BuildUrl(FindMyShoeRequest request)
        {
            try
            {
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, request.Href, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("FindMyShoe.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("FindMyShoe.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase Process(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
