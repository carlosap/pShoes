using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Library.Extensions;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Extensions;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Services
{
    public class BlogService : ParsingService
    {
        public BlogService(ICore core) : base(core)
        {
        }

        public BlogHomeResponse ParseBlogHome(IResultResponse response, IRequestParameter parameters)
        {
            var result = new BlogHomeResponse
            {
                FeaturedPost = GetFeaturedPost(response.XDocument),
                RecentPosts = GetHomeRecentPosts(response.XDocument),
                LatestTweets = GetLatestTweets(response.XDocument),
                TrendAlert = GetTrendAlert(response.XDocument),
                YouTubePlaylist = GetYouTubePlaylist(response.XDocument),
                FacebookPostHref = GetFacebookPostHref(response.XDocument)
            };
            return result;
        }

        public BlogPost ParseBlogPost(IResultResponse response, IRequestParameter parameters)
        {
            var result = new BlogPost();
            var request = (BlogPostRequest) parameters;
            result.Title = response.XDocument.FirstDescendantByClass("h2", "post-title").Value.Trim();
            var dateAuthor = response.XDocument.FirstDescendantByClass("h3", "post-date").Value.Split('|');
            result.Date = dateAuthor[0];
            result.Author = dateAuthor[1].Replace(" BY ", "").Trim();
            result.AboutTheAuthor = GetAboutTheAuthor(response.XDocument);
            result.Body = GetPostBody(response.XDocument);
            result.Image = GetPostImage(response.XDocument);
            result.Href = string.Format(Config.Urls.BlogPost, request.ID);
            result.Tags = GetPostTags(response.XDocument);
            result.RecentPosts = GetRecentPosts(response.XDocument);
            result.Archives = GetArchives(response.XDocument);
            return result;
        }

        public BlogPostList ParseBlogPostList(IResultResponse response, IRequestParameter parameters)
        {
            var result = new BlogPostList();
            var request = (BlogListRequest) parameters;
            result.RequestedArchive = request.Archive;
            result.RequestedTag = request.Tag;
            result.Title = GetTitle(response.XDocument);
            result.Tags = GetPostTags(response.XDocument);
            result.Archives = GetArchives(response.XDocument);
            result.Posts = GetPostList(response.XDocument);
            result.MoreAvailable = GetMoreAvailable(response.XDocument);            
            return result;
        }

        public BlogPostList ParseMorePosts(IResultResponse response, IRequestParameter parameters)
        {
            var result = new BlogPostList();
            var request = (MorePostsRequest) parameters;
            result.RequestedArchive = request.Archive;
            result.RequestedTag = request.Tag;
            result.Posts = GetPostList(response.XDocument);
            result.MoreAvailable = GetMoreAvailable(response.XDocument);
            return result;
        }

        public List<BlogPost> ParseRecentPosts(IResultResponse response, IRequestParameter parameters)
        {
            return GetHomeRecentPosts(response.XDocument);
        }

        private bool GetMoreAvailable(XDocument xDoc)
        {
            return xDoc.FirstDescendantByClass("a", "button viewmore").Value == "VIEW MORE";
        }

        private string GetTitle(XDocument xDoc)
        {
            return xDoc.FirstDescendantByClass("h2", "post-title").Value;
        }

        private string GetFacebookPostHref(XDocument xDoc)
        {
            return xDoc.FirstDivByClass("fb-post").AttributeValue("data-href");
        }

        private BlogPost GetFeaturedPost(XDocument xDoc)
        {
            var result = new BlogPost();
            var content = xDoc.DivByClass("blog-home-section", 1);
            var details = content.FirstDivByClass("post-details");
            result.Title = details.FirstDescendantValue("h2");
            result.Summary = details.FirstDescendantValue("p").Replace("[Read More]", "");
            result.Href = details.FirstDescendant("a").AttributeValue("href");
            var image = content.FirstDivByClass("post-img").FirstDescendant("img");
            result.Image.Src = image.AttributeValue("src");
            result.Image.Src = GetAbsoluteImagePath(result.Image.Src);
            return result;
        }

        private List<BlogPost> GetHomeRecentPosts(XDocument xDoc)
        {
            var content = xDoc.FirstDivByClass("bh-recent");
            var recentPosts = content.FirstDivByClass("recent-posts");
            var result = recentPosts.DivsByExactClass("post-block").Select(postContainer =>
            {
                var post = new BlogPost();
                var imgContainer = postContainer.FirstDivByClass("post-block-img-container");
                post.Href = imgContainer.FirstDescendant("a").AttributeValue("href");
                post.Image.Src = imgContainer.FirstDescendant("img").AttributeValue("src");
                post.Image.Src = GetAbsoluteImagePath(post.Image.Src);
                var details = postContainer.FirstDivByClass("post-block-details");
                post.Title = details.FirstDescendantValue("a");
                post.Date = details.FirstDescendantValue("span");
                post.Tags =
                    details.FirstDescendant("ul")
                        .Elements("li")
                        .Skip(1)
                        .Select(li => li.FirstDescendantValue("a"))
                        .ToList();
                post.Summary =
                    details.FirstDescendantByClass("p", "post-block-content").Value.Trim().Replace("[Read More]", "");

                return post;
            }).ToList();
            return result;
        }

        private List<string> GetLatestTweets(XDocument xDoc)
        {
            var content = xDoc.FirstDivByClass("bh-twitter");
            var carousel = content.FirstDescendantByClass("ul", "tweet-carousel");
            return carousel.Elements("li").Select(li => li.Value.Trim()).ToList();
        }

        private TrendAlert GetTrendAlert(XDocument xDoc)
        {
            var result = new TrendAlert();
            var trends = xDoc.FirstDivByClass("bh-trends");
            var link = trends.FirstDescendant("a");
            result.Href = link.AttributeValue("href");
            result.Image.Src = link.FirstDescendant("img").AttributeValue("src");
            result.Image.Src = GetAbsoluteImagePath(result.Image.Src);
            return result;
        }

        private string GetYouTubePlaylist(XDocument xDoc)
        {
            var result = "";
            var videos = xDoc.FirstDivByClass("bh-videos");
            var script = videos.Descendants("script").Skip(1).FirstOrNewXElement();
            var regex = new Regex(@"list:\s*\'([\S]*)\'", RegexOptions.ECMAScript);
            var match = regex.Match(script.Value);
            if (!match.Success) return result;
            if (match.Groups.Count > 1)
                result = match.Groups[1].Value;

            return result;
        }

        private AboutTheAuthor GetAboutTheAuthor(XDocument xDoc)
        {
            var result = new AboutTheAuthor();
            var div = xDoc.FirstDivByClass("post-author");
            result.Image.Src = div.FirstDescendant("img").AttributeValue("src");
            result.Image.Src = GetAbsoluteImagePath(result.Image.Src);
            result.Bio = div.FirstDescendantByClass("p", "author-bio").Value.Trim();
            result.Name = div.FirstDescendantByClass("span", "author-name").Value.Replace("– ", "");
            var link = div.FirstDescendantByClass("span", "author-social").FirstDescendant("a");
            result.SocialLink.Href = link.AttributeValue("href");
            result.SocialLink.Text = link.Value;
            return result;
        }

        private string GetPostBody(XDocument xDoc)
        {
            var result = "";
            var div = xDoc.FirstDivByExactClass("post-content");
            div.Descendants("p").ToList().ForEach(p => { result += p.ToString(); });
            return result;
        }

        private Image GetPostImage(XDocument xDoc)
        {
            var result = new Image();
            var div = xDoc.FirstDivByExactClass("post-content");
            result.Src = div.FirstDescendant("img").AttributeValue("src");
            result.Src = GetAbsoluteImagePath(result.Src);
            return result;
        }

        private List<string> GetPostTags(XDocument xDoc)
        {
            var div = xDoc.FirstDivByClass("post-tags");
            var result = div.Descendants("a").Select(a => a.Value).ToList();
            return result;
        }

        private List<Link> GetArchives(XDocument xDoc)
        {
            var div = xDoc.FirstDivByClass("post-archives");
            var result = div.FirstDescendant("ul").Descendants("li").Select(li =>
            {
                var link = new Link
                {
                    Text = li.FirstDescendantValue("a"),
                    Href = li.FirstDescendant("a").AttributeValue("href")
                };
                return link;
            }).ToList();
            return result;
        }

        private List<BlogPost> GetRecentPosts(XDocument xDoc)
        {
            var recent = xDoc.FirstDivByClass("left-col").FirstDivByClass("side-recent-posts");
            var result = recent.FirstDescendant("ul").Elements("li").Select(li =>
            {
                var post = new BlogPost {Image =
                {
                    
                    Src = GetAbsoluteImagePath(li.FirstDescendant("img").AttributeValue("src"))
                }};
                var link = li.FirstDescendant("a");
                post.Href = link.AttributeValue("href");
                post.Title = link.Value;
                return post;
            }).ToList();
            return result;
        }

        private List<BlogPost> GetPostList(XDocument xDoc)
        {
            var postlist = xDoc.Descendants("div").WhereAttributeEquals("class", "post-list");
            var result = postlist.Elements("div").WhereAttributeContains("class", "post-block").Select(post =>
            {
                var blogpost = new BlogPost();
                var link = post.FirstDescendant("a");
                blogpost.Href = link.AttributeValue("href");
                blogpost.Image.Src = GetAbsoluteImagePath(link.FirstDescendant("img").AttributeValue("src"));
                var details = post.FirstDivByClass("post-block-details");
                blogpost.Title = details.FirstDescendant("h5").FirstDescendantValue("a");
                blogpost.Date = details.FirstDescendantValue("span");
                blogpost.Tags =
                    details.FirstDescendantByClass("ul", "post-block-tags")
                        .Descendants("li")
                        .Where(li => !string.IsNullOrEmpty(li.FirstDescendantValue("a")))
                        .Select(li => li.FirstDescendantValue("a"))
                        .ToList();

                blogpost.Summary =
                    details.FirstDescendantByClass("p", "post-block-content")
                        .ElementValue(true)
                        .Replace("[Read More]", "");

                return blogpost;
            }).ToList();

            return result;
        }

        private static string GetAbsoluteImagePath(string imgSrc)
        {
            return !imgSrc.Contains("demandware.edgesuite.net") ? Config.Urls.BlogImageBaseUrl + imgSrc : imgSrc;
        }
    }
}
