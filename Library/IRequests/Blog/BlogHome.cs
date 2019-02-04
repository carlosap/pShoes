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

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "BlogHomeResponse")]
    public class BlogHomeResponse : IResponse
    {
        public BlogPost FeaturedPost { get; set; }
        public List<BlogPost> RecentPosts { get; set; }
        public List<string> LatestTweets { get; set; }
        public TrendAlert TrendAlert { get; set; }
        public string YouTubePlaylist { get; set; }
        public string FacebookPostHref { get; set; }        

        public BlogHomeResponse()
        {
            FeaturedPost = new BlogPost();
            RecentPosts = new List<BlogPost>();
            LatestTweets = new List<string>();
            TrendAlert = new TrendAlert();
        }
    }
}

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "BlogHome", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<BlogHomeResponse>))]
    public class BlogHome : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<BlogHomeResponse>();
            try
            {
                var request = new ExtendedComRequest(HttpRequestMethod.GET, Config.Urls.BaseUrl + Config.Urls.BlogHome, _core, _errors);
                request.OptionalRemoveScriptTags = false;
                var resultResponse = _core.RequestManager.Communicate(request);
                result.resultset = new BlogService(_core).ParseBlogHome(resultResponse, parameters);
                result.template = Config.ResponseTemplates.BlogHome;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("BlogHome.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }
    }
}