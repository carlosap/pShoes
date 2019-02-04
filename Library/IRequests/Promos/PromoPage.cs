using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Caching;
using Enums;
using Library.Cache;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.PowerReview;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Extensions;
using Library.RequestHandler;
using MadServ.Core.Models.Responses;
using Library.Models.Responses;
using System.Xml.Linq;
using System.Linq;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "PromoPage", RequestType = typeof(PromosRequest), ResponseType = typeof(Response<PromosResponse>))]
    public class PromoPage : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private PromosRequest _request;
        private string promoName;
        public PromoPage(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public PromoPage(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public PromoPage()
        {
            _errors = new List<SiteError>();
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {            
            var result = new Response<PromosResponse>();
            try
            {
                _request = (PromosRequest)parameters;
                if (_request != null && !string.IsNullOrEmpty(_request.PromoName) && Config.PromoLookup.ContainsKey(_request.PromoName))
                {
                    promoName = _request.PromoName;
                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ParseResponse(_response);

                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PromosRequest.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;

        }
        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = parameters as PromosRequest;
                var promoName = request.PromoName;
                var url = Config.PromoLookup[promoName];
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("PromosRequest.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("PromosRequest.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return new ResultResponse(); 
        }
        public Response<PromosResponse> ParseResponse(IResultResponse response)
        {
            var result = new Response<PromosResponse>();
            result.resultset.Image.Title = promoName;
            XDocument xDoc = response.XDocument;
            XNamespace ns = xDoc.Root.GetDefaultNamespace();

            var container = xDoc.Descendants(ns + "div")
                                .WhereAttributeEquals("class", "coupon-container");

            var link = container.Descendants(ns + "a").FirstOrNewXElement();
            var image = container.Descendants(ns + "img").FirstOrNewXElement();

            result.resultset.TargetUrl = link.AttributeValue("href").Replace("https://m.payless.com", "");
            result.resultset.Image.Src = image.AttributeValue("src");

            return result;
        }
    }
}



