using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using Library.Services;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Library.Cache;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "RecentPostsRequest")]
    public class RecentPostsRequest : IRequestParameter
    {
        public string Start { get; set; }
        public string Size { get; set; }
    }
}

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "RecentPostsResponse")]
    public class RecentPostsResponse : IResponse
    {
        public List<BlogPost> RecentPosts { get; set; }

        public RecentPostsResponse()
        {
            RecentPosts = new List<BlogPost>();
        }
    }
}

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "RecentPosts", RequestType = typeof(RecentPostsRequest), ResponseType = typeof(Response<RecentPostsResponse>))]
    public class RecentPosts : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<RecentPostsResponse>();
            try
            {
                var request = (RecentPostsRequest)parameters;
                var comrequest = new ExtendedComRequest(HttpRequestMethod.GET, GetUrl(request.Start, request.Size), _core, _errors) {OptionalRemoveScriptTags = false};
                var resultResponse = _core.RequestManager.Communicate(comrequest);
                result.resultset = new RecentPostsResponse() { RecentPosts = new BlogService(_core).ParseRecentPosts(resultResponse, parameters) };
                result.template = Config.ResponseTemplates.BlogHome;              
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("RecentPosts.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private string GetUrl(string start, string size)
        {
            return Config.Urls.BaseUrl + string.Format(Config.Urls.RecentPosts, start, size);
        }
    }
}