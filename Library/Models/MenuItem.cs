using System;
using System.Collections.Generic;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;
using MadServ.Core.Extensions;
using System.Xml.Linq;
using System.Web;

namespace Library.Models
{
    [Serializable]
    public class MenuItem
    {
        public string Name { get; set; }
        public string Href { get; set; }
        public List<MenuItem> Subs { get; set; }
        public Image Image { get; set; }
        public string Path { get; set; }
        public string CategoryId { get; set; }
        public bool ShowInMenu { get; set; }
        public CallOut CallOut { get; set; }
        public bool External { get; set; }

        public MenuItem()
        {
            Name = string.Empty;
            Href = string.Empty;
            Subs = new List<MenuItem>();
            Image = new Image();
            CallOut = new CallOut();
        }

        public MenuItem(Category category, string path = "") : this()
        {
            Name = category.Name;
            CategoryId = category.Id;
            ShowInMenu = category.ShowInMenu;

            if (category.HeaderMenuBanner != null)
            {
                CallOut = ParseCallOut(category.HeaderMenuBanner);
            }

            var itemId = string.Empty;

            if (!category.Name.Contains(" Catalog "))
            {
                itemId = category.Name.ToLower().Replace(" ", "-");
            }
            else
            {
                itemId = category.Id;
            }

            Href = string.Format("{0}{1}{2}{3}{4}", Config.Params.HrefPrefix, path, string.IsNullOrEmpty(path) ? string.Empty : "/", itemId, category.ParentId == "saleandclearance" ? "?pmid=saleandclearance" : string.Empty);
            Path = category.Name.ToLower();

            foreach (var sub in category.Categories)
            {
                if (!(sub.Name == null || sub.Name.Equals("Gift Cards") || sub.Name.Equals("Brands")))
                {
                    Subs.Add(new MenuItem(sub, itemId.Equals("root") ? string.Empty : string.Format("{0}{1}{2}", path, string.IsNullOrEmpty(path) ? "" : "/", itemId)));
                }
            }

            //TODO : figure out why this is repeated
            foreach (var sub in category.Categories)
            {
                if (sub.Name.Equals("Brands"))
                {
                    Subs.Add(new MenuItem(sub, itemId.Equals("root") ? string.Empty : string.Format("{0}{1}{2}", path, string.IsNullOrEmpty(path) ? "" : "/", itemId)));
                }
            }
        }

        private CallOut ParseCallOut(string callout)
        {
            var result = new CallOut();

            try
            {
                var xdoc = new XDocument();
                try
                {
                    xdoc = XDocument.Parse(callout.Replace("&", "&amp;"));
                }
                catch (Exception)
                {
                    xdoc = XDocument.Parse(callout.Replace("&", "&amp;").Replace("<span>", "").Replace("</span>", ""));
                }

                result.CallOutImage = xdoc.Descendants("div")
                                    .WhereAttributeEquals("class", "flyout content-box clearfix")
                                    .Select(a => new CallOutImage()
                                    {
                                        Src = a.Descendants("img").Attributes("src").Select(b => b.Value).FirstOrDefault(),
                                        Href = a.Descendants("a").Attributes("href").Select(b => b.Value).FirstOrDefault()
                                    }).FirstOrDefault();

                result.CallOutDescriptionTitle = xdoc.Descendants("h3")
                                            .Select(a => a.ToString()).FirstOrDefault();

                result.CallOutDescriptionBody = HttpUtility.HtmlDecode(xdoc.Descendants("p")
                                            .Select(a => a.ToString()).FirstOrDefault());

                result.CallOutButtonHref = xdoc.Descendants("a").WhereAttributeEquals("class", "button")
                                            .Attributes("href").Select(a => a.Value).FirstOrDefault();

                result.CallOutButtonText = xdoc.Descendants("a").WhereAttributeEquals("class", "button")
                                            .Select(a => a.Value).FirstOrDefault();
            }
            catch (Exception) { }

            return result;
        }

        public MenuItem(ProductSearchRefinement refiner, List<KeyValuePair<string, string>> path) : this()
        {
            var notRoot = path.Count > 1;
            var hasDescendants = refiner.Values.Any(x => x.Values.Any());

            Name = notRoot ? string.Format("Shop {0}", path.ElementAt(1).Key) : refiner.Label;

            foreach (var subSub in refiner.Values)
                if (subSub.HitCount > 0 && (subSub.Values.Any() || !hasDescendants))
                    Subs.Add(CreateMenuItem((subSub)));
        }

        private MenuItem CreateMenuItem(ProductSearchRefinementValue sub)
        {
            var item = new MenuItem();
            item.Name = sub.Label;
            item.Href = string.Format("{0}{1}", Config.Params.HrefPrefix, sub.Value);

            if (sub.Values.Any())
                foreach (var subSub in sub.Values)
                    if (subSub.HitCount > 0)
                        item.Subs.Add(CreateMenuItem((subSub)));

            return item;
        }
    }

    public class CallOut
    {
        public CallOutImage CallOutImage { get; set; }   
        public string CallOutDescriptionTitle { get; set; }
        public string CallOutDescriptionBody { get; set; }
        public string CallOutButtonHref { get; set; }
        public string CallOutButtonText { get; set; }
    }

    public class CallOutImage : Image
    {
        public string Href { get; set; }
    }
}
