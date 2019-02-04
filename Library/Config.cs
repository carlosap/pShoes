/*============================================================================
aman:config.cs
Date: 8/22/16
Description:  refactor and consolidating notes.
 * 
 * According to OCAPI documentation (section OCAPI URL syntax 17.x), the 
 * Open Commerce API uses a specific schema for its URLs, and each URL consists 
 * of a base URL and an extended URL. For development purposes the documentation
 * suggests "/s/payless/" structure. 
 * 
 * FTP File Debug: 
 *              -if you are debug / localhost make sure to have your json config file C:\ftpsites\payless\categoryidtohreflookup_1423641902869.json
 *              -See lead developer for assistance
 *              
 * 
 * HrefPrefix : is a prefix that points to the API version. 
        a) prod structure        =   /dw/api_type/
        b) development structure =   /s/payless/

 *              
=============================================================================*/
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Library.Helpers;
public partial class Config
{
    public const string ClientId = "payless";
    public const bool GWTabletEnabled = false;
    public const bool GWMobileEnabled = true;
    public const int GWMaxCart = 1800;
    public static class Keys
    {
        public static readonly string CmsClientID = "54f60b0bf3945e573bfd0a4a";
        public static readonly string CmsTabletSiteKey = "payless_tablet";
        public static readonly string CmsMobileSiteKey = "payless_mobile";
        public static readonly string Checkout = "Checkout";
        public static readonly string ClientIP = "ClientIP";
        public static readonly string DWClientCookies = "DWClientCookies";
        public static readonly string DWClientETag = "DWClientETag";
    }

    public static class CacheKeys
    {
        public static readonly string CmsInit = "Init";
        public static readonly string CmsBrandImages = "BrandImages";
        public static readonly string CmsVideos = "Videos";
        public static readonly string Menu = "Menu";    
        public static readonly string HrefLookup = "HrefLookup";
        public static readonly string RecommendProducts = "PDPRecommendedProducts-{0}";
        public static readonly string DWCategoryData = "DWCategoryData-{0}";
        public static readonly string ProductPricing = "ProductPricing-{0}";
        public static readonly string Category = "Category-{0}";
        public static readonly string ProductDetails = "PDP-{0}-{1}";
        public static readonly string Search = "SearchResponse-{0}-{1}-{2}";
        //This Categories were added due to staging clearcache and client's requirements 
        //note that the "Category-xyz" (xyz) should part of your CMS snapshots
        public static readonly string[] CmsCategoryList =
        {
            "men", 
            "women", 
            "boys", 
            "boys", 
            "girls", 
            "accessories"
        };
        
    }

    public static class CartServicesParams
    {

        public static readonly bool IsVisiblePromoHeader = false;   //true- scrapes "header-banner" from "payless.com/cart" HTML

    }
    public static class AutocompleteSearch
    {
        public static readonly string BaseUrl = "http://www.payless.com/on/demandware.store/Sites-payless-Site/default/Search-GetSuggestions?q={0}";
        public static readonly bool IsCacheEnabled = true;
        public static readonly int Cache_TTL_InMin = 1;
        public static readonly string CacheName = "AutocompleteSearch-{0}";

    }
    public static class PowerReview
    {
        public static readonly int PageSize = 10;      
        public static readonly string BaseUrl = "http://api.powerreviews.com/display/";
        public static readonly string SnapshotUrl = "http://api.powerreviews.com/display/products/snapshot?apikey={0}&merchant_id={1}&page_id={2}";
        public static readonly string ReviewsUrl = "http://api.powerreviews.com/display/reviews?apikey={0}&merchant_id={1}&page_size={2}&page_id={3}";
        public static readonly string FaceOffUrl = "http://api.powerreviews.com/display/products/face-off?apikey={0}&merchant_id={1}&page_id={2}";
        public static readonly string RatingUrl = "http://api.powerreviews.com/display/products/rating?apikey={0}&merchant_id={1}&page_id={2}";
        public static readonly string ApiKey = "5fd77048-2d1e-4a7b-a5d3-e277287ad8ea";
        public static readonly string MerchantId = "340599";
        public static readonly string MerchantGroupId = "48725";

        //----Cache Params--------
        public static readonly bool IsCacheEnabled = false;
        public static readonly int Cache_TTL_InMinutes = 5;
        public static readonly string CacheKeyReviews = "Reviews-{0}-{1}-{2}";
        public static readonly string CacheKeyAvgRate = "AvgRate-{0}";
        public static readonly string CacheKeyFaceOff = "FaceOff-{0}";
        public static readonly string CacheKeySnapShot = "SnapShot-{0}";
        
    }
    public static class Params
    {
        public static readonly TextInfo USTextInfo = new CultureInfo("en-US", false).TextInfo;

#if (DEBUG)
        public static readonly string HrefLookupDirectory = @"C:\ftpsites\payless\";
#endif

#if (PRODUCTION || STAGING || UAT || CS1 || DEV)
        public static readonly string HrefLookupDirectory = @"D:\ftpsites\payless\";
#endif
        
        public static readonly string DefaultClientIP = "198.101.135.151";
        public static readonly int DefaultRadius = 50;
        public static readonly string DefaultCountryCode = "US";
        public static readonly string DefaultDistanceUnit = "mi";
        public static readonly decimal PriceToExclude = 0M; //prices that match this will not display       
        public const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        public const string AcceptLanguage = "en-US,en;q=0.8";
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.146 Safari/537.36";        
        public const string ClientIDHeader = "x-dw-client-id";
        public const string ClientID = "3800e3b6-6dd9-4264-8cb4-ebfd6093f124";
        public const string ETagHeader = "ETag";
        public const string IfMatchHeader = "If-Match";
        public const string ResponseFormat = "json";//"xml";
        public const string HrefPrefix = "/";
        public const int ImageWidth = 200;
        public const string ClientIPHeader = "true-client-ip";
        public const string ECRMuserName = "ttec003p";
        public const string ECRMpassword = "seZ8tede";
        public const string ECRMstagingUserName = "ttec003q";
        public const string ECRMstagingPassword = "muFr2puw";
    }

    public static readonly Dictionary<string, string> PromoLookup = new Dictionary<string, string>
    {
        { "fomo-preview" , "http://www.payless.com/sms/251583021.html" }
    };

    public static readonly WebHeaderCollection DWClientHeaders = new WebHeaderCollection
    {
        {HttpRequestHeader.Accept, Config.Params.Accept},
        {HttpRequestHeader.AcceptLanguage, Config.Params.AcceptLanguage},
        {HttpRequestHeader.ContentType, string.Format("application/{0}", Config.Params.ResponseFormat)},
        {HttpRequestHeader.UserAgent, Config.Params.UserAgent},
        {Config.Params.ClientIDHeader, Config.Params.ClientID}
    };

    public static readonly Dictionary<string, string> CallOutLookup = new Dictionary<string, string>
    {
        { "alldaycomfort" , "All-Day Comfort" },
        { "extendedcalfavailable", "Extended Calf Available" },
        { "designercollection", "Designer Collection" },
        { "dexflexextraflexible", "DEXFLEX - Extra Flexible" },
        { "dextechcomfortcushion", "DEXTECH - Comfort Cushion" },
        { "dexterlitesuperlite", "DEXTER LITE - Super Lite" },
        { "extendedcalf", "Extended Calf" },
        { "extendedsizes", "Extended Sizes" },
        { "fromtherunway", "From the Runway" },
        { "lightsup", "Lights Up" },
        { "limitededition", "Limited Edition" },
        { "limitedquantities", "Limited Quantities" },
        { "oilresistant", "Oil Resistant" },
        { "onlineexclusive", "Online Exclusive" },
        { "onlineexclusiveavailable", "Online Exclusive Available" },
        { "selectcolorsonsale", "Select Colors on Sale" },
        { "skidresistant", "Skid Resistant" },
        { "slipresistant", "Slip Resistant" },
        { "steeltoe", "Steel Toe" },
        { "waterproof", "Water Proof" },
        { "widewidth", "Wide Width" },
        { "widewidthavailable", "Wide Width Available" }
    };
    public static readonly Dictionary<string, int> EnvironmentIdLookup = new Dictionary<string, int>
    {
        { ".ms." , 1 },
        { ".dev.", 2 },
        { ".staging.", 3 },
        { "t.", 5 },
        { "m.", 4 }
    };

    public static readonly List<string> ChampionSearchRedirect = new List<string>
    {
        "shirt",
        "short",
        "pant",
        "underwear",
        "bra",
        "tanks",
        "camis",
        "jacket",
        "coat",
        "hoodie",
        "sweats",
        "sweatshirt",
        "skirt",
        "capris",
        "champion"
    };

    public static class Urls
    {

        
#if (PRODUCTION)
        public static readonly string Domain = "www.payless.com";
#elif (CS1)
        public static readonly string Domain = "www.payless.com";
#elif (STAGING)
        public static readonly string Domain = "www.payless.com";
#elif (UAT)
        public static readonly string Domain = "staging.payless.com";
#elif (DEV)
        public static readonly string Domain = "www.payless.com";
#elif (DEBUG)
        public static readonly string Domain = "www.payless.com";
#else
        public static readonly string Domain = "www.payless.com";
#endif

        //==================================USE FOR REGRESSION OCAPI ONLY- SEE DWClientConfig.cs=======================================
        public static readonly bool IsApiRegressionTest = false;             //PROD NEEDS TO BE SET TO FALSE
        public static readonly string RegressionTestAPIBaseUrl = "https://staging.payless.com/s/payless/dw/shop/v17_2";
        public static readonly string RegressionTestSecureAPIBaseUrl = "https://staging.payless.com/s/payless/dw/shop/v17_2";
        //<============================================================================================================================
        public static readonly string Protocol = "https://";
        public static readonly string SecureProtocol = "https://";
        public static readonly string BaseUrl = Protocol + Domain;
        public static readonly string SecureBaseUrl = SecureProtocol + Domain;
        public static readonly string SecureBaseUrlFormatted = SecureBaseUrl;
        public static readonly string APIBaseUrl = BaseUrl + "/dw/shop/v17_2";
        public static readonly string SecureAPIBaseUrl = SecureBaseUrl + "/dw/shop/v17_2";
        public static readonly string StoreLocatorUrl = "/on/demandware.store/Sites-payless-Site/default/Stores-GetNearestStores";
        public static readonly string Stores = "/stores";
        public static readonly string CharacterPageUrl = "/characters/";
        public static readonly string EmailSignupStep1 = "/on/demandware.store/Sites-payless-Site/default/PaylessEmails-Register?format=ajax";
        public static readonly string CartAdd = "/on/demandware.store/Sites-payless-Site/default/Cart-AddProduct?format=ajax";
        public static readonly string CartDetail = "/cart";
        public static readonly string CartRemove = "/cart";
        public static readonly string CartUpdate = "/cart";
        public static readonly string CartApplyCoupon = "/cart"; //"on/demandware.store/Sites-payless-Site/default/COSinglePage-Cart"; //"/cart";
        public static readonly string CartApplyCouponOnDemandWare = "/on/demandware.store/Sites-payless-Site/default/COSinglePage-Start";
        public static readonly string CartOnDemandWareAddCoupon = "/on/demandware.store/Sites-payless-Site/default/Cart-AddCoupon";    
        public static readonly string CartUser = "/cart";
        public static readonly string CheckoutBegin = "/cart";
        public static readonly string CheckoutBeginPayPal = "/cart";
        public static readonly string CheckoutPayPalCancelCart = "/cart";
        public static readonly string CheckoutPayPalCancelBilling = "/billing";
        public static readonly string CheckoutPayPalSuccess = "/cart";
        public static readonly string CheckoutGoogleWalletReview = "/on/demandware.store/Sites-payless-Site/default/GoogleWallet-OrderSummary";
        public static readonly string CheckoutGoogleWalletPlaceOrder = "/on/demandware.store/Sites-payless-Site/default/GoogleWallet-Submit";
        public static readonly string CheckoutGuest = "/cart";
        public static readonly string CheckoutUser = "/cart";
        public static readonly string CheckoutShippingPost = "/checkout";
        public static readonly string CheckoutBillingPost = "/checkout";
        public static readonly string CheckoutShippingGet = "/shipping";
        public static readonly string CheckoutBillingGet = "/billing";
        public static readonly string CheckoutApplyCoupon = "/on/demandware.store/Sites-payless-Site/default/Cart-AddCoupon";
        public static readonly string CheckoutReviewPost = "/orderconfirmation";
        public static readonly string CheckoutReviewGet = "/revieworder";
        public static readonly string CheckoutUpdateShippingMethod = "/on/demandware.store/Sites-payless-Site/default/COShipping-UpdateShippingMethodList";
        public static readonly string CheckoutUpdateCreditCardInfo = "/on/demandware.store/Sites-payless-Site/default/COBilling-SelectCreditCard";
        public static readonly string BlogHome = "/solestyleblog";
        public static readonly string BlogPost = "/solestylepost?bid={0}";
        public static readonly string BlogPostList = "/solestylelist";
        public static readonly string RecentPosts = "/solestyleblog?start={0}&sz={1}";
        public static readonly string LoginForm = "/account";
        public static readonly string Logout = "/on/demandware.store/Sites-payless-Site/default/Login-Logout";
        public static readonly string ForgotPasswordForm = "/on/demandware.store/Sites-payless-Site/default/Account-PasswordResetDialog?format=ajax";
        public static readonly string ResetPasswordForm = "/setpassword";
        public static readonly string ResetPassword = "/setpassword";
        public static readonly string OrderLookupForm = "/orderstatus";
        public static readonly string OrderLookup = "/orderstatus";
        public static readonly string ShippingOptionsAPIPart1 = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/COShipping-SelectShippingMethod?stateCode={0}&postalCode={1}&city={2}&shippingMethodID={3}&csrf_token={4}";
        public static readonly string ShippingOptionsAPIPart2 = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/COBilling-UpdateSummary";
        public static readonly string ApplyBillingCouponPart1 = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/Cart-AddCoupon";
        public static readonly string ApplyBillingCouponPart2 = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/COBilling-UpdateSummaryBill";


        public static readonly string GetAdditionalPLPData = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/Product-GetPLPJSON";
        public static readonly string GetAdditionalPLPDataCache = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/Product-GetPLPJSONCache";
        public static readonly string GetAdditionalPDPData = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/Product-GetRecsJSON?pid=";
        public static readonly string GetProductPricing = SecureBaseUrlFormatted + "/on/demandware.store/Sites-payless-Site/default/Product-GetProdPriceJSON?pid={0}";     
        public static readonly string ProductRatingStarsTemplate = "http://reviews.payless.com/0556/{0}_0/5/rating.gif";
        public static readonly string ProductRatingBarsTemplate = "http://reviews.payless.com/0556/{0}_0/5/rating{1}.gif";
        public static readonly string DWImageOptimizerUrl = "http://demandware.edgesuite.net/sits_pod15/dw/image/v2/AAPR_PRD";
        public static readonly string DefaultGiftCardImage = "http://demandware.edgesuite.net/sits_pod15/dw/image/v2/AAPR_PRD/on/demandware.static/Sites-payless-Site/Sites-payless-catalog/default/v1393802487471/images/hi-res/800033_4_1400x1400.jpg?sw=200";
        public static readonly string BrandImageTemplate = "http://demandware.edgesuite.net/aapr_prd/on/demandware.static/Sites-payless-Site/Sites/default/images/brands/{0}";
        public static readonly string BlogImageBaseUrl = "http://demandware.edgesuite.net/aapr_prd";
        public static readonly string CategoryImageTemplate = "http://demandware.edgesuite.net/sits_pod15/dw/image/v2/AAPR_PRD/on/demandware.static/Sites-payless-Site/Sites-payless-catalog/default/images/hi-res/{0}1400x1400.jpg?sw=100";
        public static readonly string DefaultMobileBaseUrl = "https://payless.ms.com";
    }

    public static class DWPath
    {
        public static readonly string Menu = "/categories/{0}";
        public static readonly string Search = "/product_search";
        public static readonly string ProductDetail = "/products";
        public static readonly string ProductListExtension = "/products";
        public static readonly string ProductDetailExtension = "/products";
        public static readonly string StoreLocator = "/stores";
    }

    public enum TemplateEnum
    {
        GeneralError = 0,
        CartMini,
        CartDetail,
        CheckoutBegin,
        CheckoutShipping,
        CheckoutBilling,
        CheckoutReview,
        CheckoutConfirmation,
        Logout,
        Login,
        OrderHistory,
        OrderDetail,
        AccountDashboard,
        PayPalRedirect,
        FindMyPerfectShoe
    }

    public static class ResponseTemplates
    {
        public static readonly string Menu = "menu";
        public static readonly string ProductList = "productlist";
        public static readonly string ProductDetail = "productdetail";
        public static readonly string ProductListExtension = "productlistextension";
        public static readonly string ProductDetailExtension = "productdetailextension";
        public static readonly string BlogHome = "bloghome";
        public static readonly string BlogPost = "blogpost";
        public static readonly string BlogPostList = "blogpostlist";
        public static readonly string MorePosts = "moreposts";
    }

    public static class Constants
    {

        public static readonly string ResetPasswordError = "Please enter a valid Password.";
        public static readonly string GenericError = "The application has encountered an unknown error. Please try again later...";
        public static readonly string ShipToStore = "shipToStore";
        public static readonly string CouponInvalidMessage = "Coupon \"{0}\" is invalid";
        public static readonly string CouponAppliedMessage = "Coupon \"{0}\" has been applied to your order";
        public static readonly string CouponNAMessage = "Coupon \"{0}\" is not applicable";
    }
    public static readonly EndpointAddress ECRMstagingEndpoint = new EndpointAddress("https://osbqa.payless.com/osb/PSS_ICC_ECRMIntegration/ProxyService/IB_Loyalty_proxy");
    public static readonly EndpointAddress ECRMendpoint = new EndpointAddress("https://osb.payless.com/osb/PSS_ICC_ECRMIntegration/ProxyService/IB_Loyalty_proxy");
    public static readonly CustomBinding ECRMbinding = APIHelper.GetECRMbinding();
}