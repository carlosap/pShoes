using System;
using System.Collections.Generic;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class LandingPageItem
    {
        public List<KeyValuePair<string, string>> Links { get; set; }
        public string Description { get; set; }
        public Image Image { get; set; }
        public string PinterestLink { get; set; }
        public string FacebookLink { get; set; }
        public string TweeterLink { get; set; }

        public string PageDetailUrl { get; set; }

        public bool isHidden { get; set; }
        public string Size { get; set; } // half/full/quarter

        public LandingPageItem()
        {
            Links = new List<KeyValuePair<string, string>>();
            Image = new Image();
        }
    }
}
