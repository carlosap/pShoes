using Enums;
using Library.Models.Requests;
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
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "MorePostsRequest")]
    public class MorePostsRequest : IRequestParameter
    {
        public string Tag { get; set; }
        public string Archive { get; set; }
        public string Start { get; set; }
        public string Size { get; set; }
        public MorePostsRequest()
        {
            Tag = "";
            Archive = "";
            Start = "";
            Size = "";
        }
    }
}

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "MorePosts", RequestType = typeof(MorePostsRequest), ResponseType = typeof(Response<Library.Models.BlogPostList>))]
    public class MorePosts : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private MorePostsRequest _request;
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<Models.BlogPostList>();
            _request = (MorePostsRequest)parameters ?? new MorePostsRequest();
            try
            {
                var request = new ExtendedComRequest(HttpRequestMethod.GET, GetUrl(), _core, _errors);
                request.OptionalRemoveScriptTags = false;
                var resultResponse = _core.RequestManager.Communicate(request);
                result.resultset = new BlogService(_core).ParseMorePosts(resultResponse, parameters);
                result.template = Config.ResponseTemplates.MorePosts;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("BlogPostList.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private string GetUrl()
        {
            var result = Config.Urls.BaseUrl + Config.Urls.BlogPostList;
            var querystring = "";

            if (!string.IsNullOrEmpty(_request.Tag))
                querystring += "tid=" + _request.Tag;
            if (!string.IsNullOrEmpty(_request.Archive))
            {
                if (querystring != "")
                    querystring += "&";

                querystring += "aid=" + _request.Archive;
            }

            if (!string.IsNullOrEmpty(_request.Start))
            {
                if (querystring != "")
                    querystring += "&";

                querystring += "start=" + _request.Start;
            }

            if (!string.IsNullOrEmpty(_request.Size))
            {
                if (querystring != "")
                    querystring += "&";

                querystring += "sz=" + _request.Size;
            }

            if (querystring != "")
                result += "?" + querystring;

            return result;
        }
    }
}