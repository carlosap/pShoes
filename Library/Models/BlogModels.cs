using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Library.Models
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "BlogPost")]
    public class BlogPost
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string Href { get; set; }
        public Image Image { get; set; }
        public List<string> Tags { get; set; }
        public string Date { get; set; }
        public AboutTheAuthor AboutTheAuthor { get; set; }
        public List<BlogPost> RecentPosts { get; set; }
        public List<Link> Archives { get; set; }

        public BlogPost()
        {
            Image = new Image();
            Tags = new List<string>();
            AboutTheAuthor = new AboutTheAuthor();
            RecentPosts = new List<BlogPost>();
            Archives = new List<Link>();
        }
    }

    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "BlogPostList")]
    public class BlogPostList
    {
        public string Title { get; set; }
        public List<BlogPost> Posts { get; set; }
        public List<string> Tags { get; set; }
        public List<Link> Archives { get; set; }
        public bool MoreAvailable { get; set; }

        public string RequestedTag { get; set; }
        public string RequestedArchive { get; set; }

        public BlogPostList()
        {
            Posts = new List<BlogPost>();
            Tags = new List<string>();
            Archives = new List<Link>();
        }
    }

    public class AboutTheAuthor
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public Image Image { get; set; }
        public Link SocialLink { get; set; }

        public AboutTheAuthor()
        {
            Image = new Image();
            SocialLink = new Link();
        }
    }

    public class TrendAlert
    {
        public string Href { get; set; }
        public Image Image { get; set; }

        public TrendAlert()
        {
            Image = new Image();
        }
    }

    public class Link
    {
        public string Text { get; set; }
        public string Href { get; set; }
    }
}
