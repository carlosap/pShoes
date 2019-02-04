using System.Collections.Generic;

namespace Library.Models
{
    public class Banner
    {
        public string Image { get; set; }
        public string Href { get; set; }
        public string Exclusion { get; set; }
    }

    public class ContentRow
    {
        public List<ContentSlot> Slots { get; set; }

        public ContentRow()
        {
            Slots = new List<ContentSlot>();
        }
    }

    public class ContentSlot
    {
        public string Type { get; set; }
        public string Width { get; set; }
    }

    public class FeatureSlider : ContentSlot
    {
        public List<Feature> Slides { get; set; }
        public int Interval { get; set; }

        public FeatureSlider()
        {
            Slides = new List<Feature>();
            Interval = 2500;
        }
    }

    public class Feature : ContentSlot
    {
        public string Image { get; set; }
        public string Href { get; set; }
    }

    public class ProductSlider : ContentSlot
    {
        public List<ProductSlide> Slides { get; set; }
        public int Interval { get; set; }

        public ProductSlider()
        {
            Slides = new List<ProductSlide>();
            Interval = 2500;
        }
    }

    public class ProductSlide
    {
        public string Id { get; set; }
        public string Image { get; set; }
        public string Review { get; set; }
        public string Reviewer { get; set; }
        public string Title { get; set; }
        public string Brand { get; set; }
        public string Rating { get; set; }
        public string RegPrice { get; set; }
        public string SalePrice { get; set; }
        public string Href { get; set; }
    }

}
