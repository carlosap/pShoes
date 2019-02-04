using System;
using MadServ.Core.Models;
using System.Collections.Generic;

namespace Library.Models
{
    [Serializable]
    public class SizeOption : Option
    {
        public bool IsInStock { get; set; }
        public string ShortName { get; set; }
        public string SizeSegment { get; set; }
        public string SizeLabel { get; set; }
        public string WidthLabel { get; set; }
        public string Price { get; set; }
        public string ExclusiveMsg { get; set; }
        public ProductVariant.Prices PriceRange { get; set; }
    }

    [Serializable]
    public class TabletSizeOption : SizeOption
    {
        public List<SizeOption> Widths { get; set; }

        public TabletSizeOption()
        {
            Widths = new List<SizeOption>();
            PriceRange = new ProductVariant.Prices();
        }
    }
}
