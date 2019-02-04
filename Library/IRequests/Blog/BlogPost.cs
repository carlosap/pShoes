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
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "BlogPostRequest")]
    public class BlogPostRequest : IRequestParameter
    {
        public string ID { get; set; }
    }
}

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "BlogPost", RequestType = typeof(BlogPostRequest), ResponseType = typeof(Response<Library.Models.BlogPost>))]
    public class BlogPost : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private BlogPostRequest _request;
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<Models.BlogPost>();
            _request = (BlogPostRequest)parameters;

            try
            {
                var request = new ExtendedComRequest(HttpRequestMethod.GET,
                    string.Format(Config.Urls.BaseUrl + Config.Urls.BlogPost, _request.ID), _core, _errors)
                {
                    OptionalRemoveScriptTags = false
                };
                var resultResponse = _core.RequestManager.Communicate(request);
                result.resultset = new BlogService(_core).ParseBlogPost(resultResponse, parameters);
                result.template = Config.ResponseTemplates.BlogPost;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("BlogPost.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

            return result;
        }
    }
}