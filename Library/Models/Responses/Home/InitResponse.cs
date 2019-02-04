using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using MadServ.Core.Helpers;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "InitResponse")]
    public class InitResponse : IResponse
    {
        public List<MenuItem> Menu { get; set; }
        public List<AllBrands> AllBrandsBreakdown { get; set; }
        public string ID { get; set; }
        public string SessionID { get; set; }
        public int CartItemCount { get; set; }
        public double CartTotalAmount { get; set; }
        public string CriteoCustomerId { get; set; }
        public bool IsLoggedIn { get; set; }
        public string ClientIP { get; set; }
        public string PromoHeader { get; set; }
        public bool PingdomSuccess { get; set; }
        public double GeoLat { get; set; }
        public double GeoLon { get; set; }
        public string Environment { get; set; }

        //phone
        public List<MadCms.CmsImage> FeatureImages { get; set; }
        public List<MadCms.CmsImage> FeatureImagesB { get; set; }
        public List<MadCms.CmsImage> Banners { get; set; }
        public List<MadCms.CmsImage> SecondaryBanners { get; set; }
        //public List<MadCms.CmsImage> HorizontalBanners { get; set; }
        //public List<MadCms.CmsImage> VerticalBanners { get; set; }
        //public List<MenuItem> MobileMenu { get; set; }
        public Dictionary<string, object> ScriptToggles { get; set; }
        
        //tablet
        public List<Banner> HeroBanners { get; set; }
        public List<Banner> TabletBanners { get; set; }
        public List<ContentRow> ContentRows { get; set; }
        public string CheckoutPromoMessage { get; set; }

        public InitResponse()
        {
            Menu = new List<MenuItem>();
            AllBrandsBreakdown = new List<AllBrands>();
            PingdomSuccess = true;


            FeatureImages = new List<MadCms.CmsImage>();
            FeatureImagesB = new List<MadCms.CmsImage>();
            Banners = new List<MadCms.CmsImage>();
            SecondaryBanners = new List<MadCms.CmsImage>();
            //HorizontalBanners = new List<MadCms.CmsImage>();
            //VerticalBanners = new List<MadCms.CmsImage>();
            Menu = new List<MenuItem>();
            
            
            HeroBanners = new List<Banner>();
            TabletBanners = new List<Banner>();
            ContentRows = new List<ContentRow>();
        }
    }

    public class AllBrands
    {
        public string GroupName { get; set; }
        public List<MenuItem> Brands { get; set; }
    }
}
