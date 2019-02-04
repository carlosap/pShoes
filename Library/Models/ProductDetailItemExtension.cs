using System;
namespace Library.Models
{
    [Serializable]
    public class ProductDetailItemExtension
    {
        public string SizeSegment { get; set; }
        public string ExclusiveMsg { get; set; }
        public string Brand { get; set; }
        public ProductVariant.Prices PriceRange { get; set; }

        public ProductDetailItemExtension()
        {
            PriceRange = new ProductVariant.Prices();
        }
    }
}
