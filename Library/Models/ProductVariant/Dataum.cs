using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models.ProductVariant
{
    public class Datum
    {
        public string currency { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string page_description { get; set; }
        public string page_keywords { get; set; }
        public string page_title { get; set; }
        public float price { get; set; }
        public Prices prices { get; set; }
        public string short_description { get; set; }
        public Type type { get; set; }
        public bool c_InvodoVideoExists { get; set; }
        public string c_PR_ReviewURL { get; set; }
        public string c_StarRating { get; set; }
        public string c_StarRatingBase { get; set; }
        public string c_StarRatingCount { get; set; }
        public bool c_availableForInStorePickup { get; set; }
        public string c_bvAverageRating { get; set; }
        public string c_bvRatingRange { get; set; }
        public string c_bvRefinementRating { get; set; }
        public string c_bvReviewCount { get; set; }
        public string c_careInstructions { get; set; }
        public int c_category { get; set; }
        public int c_class { get; set; }
        public string c_color { get; set; }
        public string c_department { get; set; }
        public string c_displayBrand { get; set; }
        public bool c_findinstoreindicator { get; set; }
        public string c_gender { get; set; }
        public bool c_isGiftCard { get; set; }
        public bool c_isSale { get; set; }
        public string c_lot { get; set; }
        public string c_lotDescription { get; set; }
        public string c_lotType { get; set; }
        public bool c_notSellable { get; set; }
        public string[] c_productFlags { get; set; }
        public string c_refinementColor { get; set; }
        public bool c_rrRecommendable { get; set; }
        public bool c_shiptostoreprod { get; set; }
        public string c_size { get; set; }
        public string c_sizeByAge { get; set; }
        public string c_sizeSegment { get; set; }
        public string c_width { get; set; }
    }
}
