using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Enums;
using Library.Models.Requests;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.RequestHandler;
using MadServ.Core.Models.Responses;
using Library.Cache;
using Library.Models.Autocomplete;
using Library.Models.Responses.Autocomplete;
using Newtonsoft.Json;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "AutocompleteSearch", RequestType = typeof(AutocompleteSearchRequest), ResponseType = typeof(Response<AutoCompleteSearchResponse>))]
    public class AutocompleteSearch : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private AutocompleteSearchRequest _request;
        public AutocompleteSearch(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public AutocompleteSearch(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public AutocompleteSearch()
        {
            _errors = new List<SiteError>();
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<AutoCompleteSearchResponse>();
            try
            {
                _request = (AutocompleteSearchRequest)parameters;
                var cacheKey = string.Format(Config.AutocompleteSearch.CacheName, _request.SearchToken);
                result = CacheMemory.Get<Response<AutoCompleteSearchResponse>>(cacheKey);
                if (result == null || result.resultset.SearchItem.Items.Count == 0)
                {
                    var communicationRequest = BuildUrl(parameters);
                    _response = Communicate(communicationRequest);
                    result = ParseResponse(_response);
                    if (result.resultset.SearchItem.Items.Count > 0)
                    {
                        if (Config.AutocompleteSearch.IsCacheEnabled && _request.SearchToken.Length > 2)
                        {
                            CacheMemory.SetAndExpiresMinutesAsync(cacheKey, result, Config.AutocompleteSearch.Cache_TTL_InMin);                       
                        }      
                    }
                }
            }
            catch (Exception)
            {
                //don't fail/performace the UI because mad search errors.
                //allow user to search again.
                return new Response<AutoCompleteSearchResponse>();
            }
            return result;
        }
        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                var request = parameters as AutocompleteSearchRequest;
                var url = string.Format(Config.AutocompleteSearch.BaseUrl,_request.SearchToken); 
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("AutocompleteSearch.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("AutocompleteSearch.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }
            return new ResultResponse(); 
        }
        public Response<AutoCompleteSearchResponse> ParseResponse(IResultResponse response)
        {
            var result = new Response<AutoCompleteSearchResponse>();
            result.resultset.SearchItem.Items = JsonConvert.DeserializeObject<List<SearchItem>>(response.RawData);
            return result;
        }
    }
}

