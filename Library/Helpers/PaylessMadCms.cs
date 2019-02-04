using Library.Models;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Library.Cache;

namespace Library.Helpers
{
    public class PaylessMadCms
    {


        private dynamic _homepageData { get; set; }
        private IDictionary<string, object> _homepageDataProperties { get; set; }
        private dynamic _heroBanners { get; set; }
        private IDictionary<string, object> _heroBannersDictionary { get; set; }
        private MadCms _tabletCms { get; set; }
        private MadCms _mobileCms { get; set; }
        private ICore _core { get; set; }

        public PaylessMadCms(ICore core)
        {
            _core = core;
            _tabletCms = new MadCms(Config.Keys.CmsClientID, Config.Keys.CmsTabletSiteKey, EnvironmentHelper.GetCmsEnvironment(core.Context));
            _mobileCms = new MadCms(Config.Keys.CmsClientID, Config.Keys.CmsMobileSiteKey, EnvironmentHelper.GetCmsEnvironment(core.Context));
        }

        public NameValueCollection CategoryImages()
        {
            var result = new NameValueCollection();
            var menuImages = _mobileCms.GetMetaData("menu_images");
            foreach (var category in menuImages)
                result.Add(category.Value.category_id,
                    string.Format(Config.Urls.CategoryImageTemplate, category.Value.image_id));

            return result;
        }

        public InitResponse GetInitData(DateTime? forwardDate = null)
        {
            var result = new InitResponse();
            result = CacheMemory.Get<InitResponse>(Config.CacheKeys.CmsInit);
            if (result == null || result.SessionID == null)
            {

                _homepageData = _tabletCms.GetMetaData("home_page_data");
                _homepageDataProperties = (IDictionary<string, object>)_homepageData;
                _heroBanners = _tabletCms.GetMetaData("hero_banners");
                _heroBannersDictionary = (IDictionary<string, object>)_heroBanners;
                try
                {
                    if (!forwardDate.HasValue)
                    {
                        forwardDate = DateTime.Now;
                    }
                }
                catch (Exception)
                {
                    forwardDate = DateTime.Now;
                }

                result.TabletBanners = GetTabletBanners(forwardDate.Value);
                result.HeroBanners = GetHeroBanners(forwardDate.Value);
                result.ContentRows = GetContentRows(forwardDate.Value);
                result.FeatureImages = _mobileCms.GetImages("feature_images");
                result.FeatureImagesB = _mobileCms.GetImages("feature_images_b");
                result.Banners = _mobileCms.GetImages("banners");
                result.CheckoutPromoMessage = GetCheckoutPromoMessage();
                result.SecondaryBanners = _mobileCms.GetImages("banners-b");
                var scriptToggleMetaData = _mobileCms.GetMetaData("script_toggles");

                try
                {
                    if (scriptToggleMetaData != null)
                    {
                        result.ScriptToggles = new Dictionary<string, object>(scriptToggleMetaData);
                    }
                }
                catch (Exception)
                {
                    result.ScriptToggles = new Dictionary<string, object>();
                }

                SetPlpPromosInCache();

                CacheMemory.SetAndExpiresMinutesAsync(Config.CacheKeys.CmsInit, result, 5);
            }
            return result;
        }

        private void SetPlpPromosInCache()
        {
            var result = new Dictionary<string, PromoSlot>();
            try
            {
                var promos = _tabletCms.GetMetaData("plp_promos");
                if (promos != null)
                {
                    foreach (var item in promos)
                    {
                        var promoSlot = new PromoSlot();
                        var _itemProperties = (IDictionary<string, object>)item.Value;

                        if (_itemProperties.ContainsKey("video"))
                            promoSlot.VideoID = item.Value.video;

                        if (_itemProperties.ContainsKey("mobileImage"))
                            promoSlot.MobileImage = item.Value.mobileImage;

                        if (_itemProperties.ContainsKey("tabletImage"))
                            promoSlot.TabletImage = item.Value.tabletImage;

                        result.Add(item.Key, promoSlot);
                    }
                }
                CacheMemory.SetAndExpiresMinutesAsync(Config.CacheKeys.CmsVideos, result, 15);
            }
            catch { }
        }

        public string GetCheckoutPromoMessage()
        {
            var result = "";
            try
            {
                var cmsData = _tabletCms.GetMetaData("checkout-promo-message");
                var cmsDataProperties = (IDictionary<string, object>)cmsData;
                if (cmsData != null && cmsDataProperties.ContainsKey("text"))
                    result = cmsData.text.text;
            }
            catch { } //make sure this doesn't stop the site
            return result;
        }

        public CategoryResponse GetCategoryData(string categoryName, DateTime? forwardDate = null)
        {
            var result = new CategoryResponse();
            if (!forwardDate.HasValue)
                forwardDate = DateTime.Now;

            result.TabletBanners = GetTabletBanners(forwardDate.Value, categoryName);
            result.HeroBanners = GetHeroBanners(forwardDate.Value);
            result.ContentRows = GetContentRows(forwardDate.Value, categoryName);
            return result;
        }

        private List<Banner> GetTabletBanners(DateTime forwardDate, string category = "home_page_data")
        {
            var result = new List<Banner>();
            var cmsData = _tabletCms.GetMetaData(category);
            var cmsDataProperties = (IDictionary<string, object>)cmsData;
            if (cmsData != null && cmsDataProperties.ContainsKey("banners"))
                result = ParseBanners(cmsData.banners, forwardDate);

            return result;
        }

        private List<Banner> GetHeroBanners(DateTime forwardDate)
        {
            var result = new List<Banner>();
            if (_heroBanners != null)
                result = ParseBanners(_heroBanners, forwardDate);

            return result;
        }

        private List<Banner> ParseBanners(dynamic banners, DateTime forwardDate)
        {
            var result = new List<Banner>();

            if (banners != null)
            {
                foreach (var banner in banners)
                {
                    var bannerItem = banner.Value;
                    var bannerItemProperties = (IDictionary<string, object>)bannerItem;

                    var start = DateTime.MinValue;
                    var end = DateTime.MaxValue;

                    if (bannerItemProperties.ContainsKey("start"))
                    {
                        if (!string.IsNullOrEmpty(bannerItem.start))
                        {
                            start = DateTime.Parse(bannerItem.start);
                        }
                    }
                    if (bannerItemProperties.ContainsKey("end"))
                    {
                        if (!string.IsNullOrEmpty(bannerItem.end))
                        {
                            end = DateTime.Parse(bannerItem.end);
                        }
                    }

                    if (start <= forwardDate && end >= forwardDate)
                    {
                        var newBanner = new Banner();

                        if (bannerItemProperties.ContainsKey("href"))
                        {
                            newBanner.Href = bannerItem.href;
                        }
                        if (bannerItemProperties.ContainsKey("image"))
                        {
                            newBanner.Image = bannerItem.image;
                        }

                        if (bannerItemProperties.ContainsKey("exclusion"))
                        {
                            newBanner.Exclusion = bannerItem.exclusion;
                        }

                        result.Add(newBanner);
                    }
                }
            }

            return result;
        }

        private List<ContentRow> GetContentRows(DateTime forwardDate, string category = "home_page_data")
        {
            var result = new List<ContentRow>();
            var cmsData = _tabletCms.GetMetaData(category);
            var cmsDataProperties = (IDictionary<string, object>)cmsData;
            if (cmsData != null && cmsDataProperties.ContainsKey("rows"))
            {
                foreach (var row in cmsData.rows)
                {
                    var rowItem = row.Value;

                    var contentRow = new ContentRow();

                    foreach (var slot in rowItem)
                    {
                        var slotItem = slot.Value;
                        var slotItemProperties = (IDictionary<string, object>)slotItem;

                        var start = DateTime.MinValue;
                        var end = DateTime.MaxValue;

                        if (slotItemProperties.ContainsKey("start"))
                        {
                            if (!string.IsNullOrEmpty(slotItem.start))
                            {
                                start = DateTime.Parse(slotItem.start);
                            }
                        }
                        if (slotItemProperties.ContainsKey("end"))
                        {
                            if (!string.IsNullOrEmpty(slotItem.end))
                            {
                                end = DateTime.Parse(slotItem.end);
                            }
                        }

                        if (start <= forwardDate && end >= forwardDate)
                        {
                            var contentSlot = new ContentSlot();

                            var type = "";
                            var width = "";

                            if (slotItemProperties.ContainsKey("type"))
                            {
                                type = slotItem.type;
                            }

                            if (slotItemProperties.ContainsKey("width"))
                            {
                                width = slotItem.width;
                            }

                            switch (type)
                            {
                                case "feature-slider":

                                    contentSlot = ParseFeatureSlider(slot);

                                    break;
                                case "shoe-finder":

                                    //nothing really needs to be parsed for this, just type and width below

                                    break;
                                case "feature":

                                    if (slotItemProperties.ContainsKey("feature"))
                                    {
                                        contentSlot = ParseFeature(slotItem.feature);
                                    }

                                    break;
                                case "product-slider":

                                    contentSlot = ParseProductSlider(slot);

                                    break;
                            }

                            contentSlot.Type = type;
                            contentSlot.Width = width;

                            contentRow.Slots.Add(contentSlot);

                        }
                    }

                    result.Add(contentRow);
                }
            }

            return result;
        }

        public Feature ParseFeature(dynamic feature)
        {
            var result = new Feature();
            var featureProperties = (IDictionary<string, object>)feature;
            if (featureProperties.ContainsKey("image"))
                result.Image = feature.image;
            if (featureProperties.ContainsKey("href"))
                result.Href = feature.href;

            return result;
        }

        public FeatureSlider ParseFeatureSlider(dynamic slot)
        {
            var result = new FeatureSlider();
            var slotItem = slot.Value;
            var slotProperties = (IDictionary<string, object>)slotItem;
            if (slotProperties.ContainsKey("interval"))
                result.Interval = Convert.ToInt32(slotItem.interval);

            if (!slotProperties.ContainsKey("features")) return result;
            foreach (var feature in slotItem.features)
                result.Slides.Add(ParseFeature(feature.Value));

            return result;
        }

        public ProductSlider ParseProductSlider(dynamic slot)
        {
            var result = new ProductSlider();

            var slotItem = slot.Value;
            var slotProperties = (IDictionary<string, object>)slotItem;

            if (slotProperties.ContainsKey("interval"))
                result.Interval = Convert.ToInt32(slotItem.interval);

            if (slotProperties.ContainsKey("products"))
            {
                foreach (var product in slotItem.products)
                {
                    var slide = new ProductSlide();
                    var productItem = product.Value;
                    var productProperties = (IDictionary<string, object>)productItem;

                    if (productProperties.ContainsKey("id"))
                        slide.Id = productItem.id;

                    if (productProperties.ContainsKey("image"))
                        slide.Image = productItem.image;

                    if (productProperties.ContainsKey("review"))
                        slide.Review = productItem.review;

                    if (productProperties.ContainsKey("reviewer"))
                        slide.Reviewer = productItem.reviewer;

                    if (productProperties.ContainsKey("title"))
                        slide.Title = productItem.title;

                    if (productProperties.ContainsKey("brand"))
                        slide.Brand = productItem.brand;

                    if (productProperties.ContainsKey("rating"))
                        slide.Rating = productItem.rating;

                    if (productProperties.ContainsKey("href"))
                        slide.Href = productItem.href;

                    result.Slides.Add(slide);
                }
            }

            return result;
        }

        public Dictionary<string, string> BrandImageLookup()
        {
            var result = new Dictionary<string, string>();
            result = CacheMemory.Get<Dictionary<string, string>>(Config.CacheKeys.CmsBrandImages);
            if (result == null || result.Count == 0)
            {
                var brandImagesData = _tabletCms.GetMetaData("brand_images");
                foreach (var brand in brandImagesData)
                {
                    var properties = (IDictionary<string, object>)brand.Value;
                    if (properties.ContainsKey("brand") && properties.ContainsKey("image"))
                        result.Add(brand.Value.brand, brand.Value.image);
                }
                CacheMemory.SetAndExpiresMinutesAsync(Config.CacheKeys.CmsBrandImages, result, 15);
            }
            return result;
        }

        public List<MenuItem> GetMobileMenu()
        {
            var result = new List<MenuItem>();
            var cmsMenu = _mobileCms.GetMetaData("mobile_menu");
            if (cmsMenu == null) return result;
            foreach (var item in cmsMenu)
            {
                var menuItem = ParseMenuItem(item);
                result.Add(menuItem);
            }
            return result;
        }

        private MenuItem ParseMenuItem(dynamic item)
        {
            var result = new MenuItem();

            var itemValue = item.Value;
            var itemProperties = (IDictionary<string, object>)itemValue;

            if (itemProperties.ContainsKey("name"))
                result.Name = itemValue.name;

            if (itemProperties.ContainsKey("categoryID"))
                result.CategoryId = itemValue.categoryID;

            if (itemProperties.ContainsKey("href"))
                result.Href = itemValue.href;

            if (itemProperties.ContainsKey("image"))
                result.Image.Src = itemValue.image;

            if (itemProperties.ContainsKey("path"))
                result.Path = itemValue.path;

            if (itemProperties.ContainsKey("subs"))
            {
                foreach (var sub in itemValue.subs)
                {
                    var subItem = ParseMenuItem(sub);

                    result.Subs.Add(subItem);
                }
            }

            result.ShowInMenu = true;
            if (itemProperties.ContainsKey("enabled"))
            {
                bool enabled;
                if (bool.TryParse(itemValue.enabled, out enabled))
                    result.ShowInMenu = enabled;
            }

            if (!itemProperties.ContainsKey("external")) return result;
            bool external;
            if (bool.TryParse(itemValue.external, out external))
                result.External = external;

            return result;
        }

        public class PromoSlot
        {
            public string VideoID { get; set; }
            public string MobileImage { get; set; }
            public string TabletImage { get; set; }
        }
    }
}
