using Enums;
using Library.Helpers;
using Library.Models;
using Library.Models.Responses;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Library.Services
{
    public class CartService : IService
    {
        private readonly ICore _core;
        public List<SiteError> _errors { get; set; }
        private Cart _cart;
        private XDocument _xDoc;
        private XNamespace _ns;

        private PaylessSession _session;

        public CartService(ICore core)
        {
            _core = core;
            _session = new PaylessSession(core);
        }

        public IResponseBase Process(IResultResponse xResponse, IRequestParameter parameters, List<SiteError> errors)
        {
            _errors = errors;
            _cart = _session.GetCheckout().Cart;
            _xDoc = xResponse.XDocument;
            if (_xDoc != null)
            {
                _ns = _xDoc.Root.GetDefaultNamespace();
            }

            var result = xResponse.Template.Method(xResponse, parameters);

            if (!_errors.Any(x => x.Type != ErrorType.UserActionRequired) && result.GetType() == typeof(Response<CartResponse>))
            {
                var cart = ((Response<CartResponse>)result).resultset;
                cart.Template = xResponse.Template.TemplateName.ToString();

                if (xResponse.Template.TemplateName.Equals(Config.TemplateEnum.CartDetail))
                {
                    SaveCart(cart.Cart);
                }
                else
                {
                    SaveCartItemCount(cart.Cart);
                }
            }


            return result;
        }

        public IResponseBase ParseCartMini(IResultResponse response, IRequestParameter parameters)
        {
            Response<CartResponse> result = new Response<CartResponse>();

            try
            {
                var cartItemCount = 0;
                var items = _xDoc.Descendants(_ns + "div")
                                 .WhereAttributeEquals("class", "mini-cart-product")
                                 .Select(x =>
                                     {
                                         var name = x.Elements(_ns + "div")
                                                     .WhereAttributeEquals("class", "mini-cart-name")
                                                     .FirstOrNewXElement()
                                                     .ElementValue();

                                         var qtyStr = x.Elements(_ns + "div")
                                                       .WhereAttributeEquals("class", "mini-cart-pricing")
                                                       .FirstOrNewXElement()
                                                       .Elements(_ns + "span")
                                                       .WhereAttributeEquals("class", "value")
                                                       .FirstOrNewXElement()
                                                       .ElementValue();
                                         var qty = 0;
                                         Int32.TryParse(qtyStr, out qty);
                                         cartItemCount += qty;

                                         return new CartItem
                                         {
                                             Name = name,
                                             Quantity = qty
                                         };
                                     })
                                 .ToList();

                result.resultset.Cart.CartItems = items;
                result.resultset.Cart.CartItemCount = cartItemCount;
            }
            catch (Exception ex)
            {

                _errors.Add(ex.Handle(
                   "MadServ.CartService.ParseCartMini",
                   ErrorSeverity.FollowUp,
                   ErrorType.Parsing
                   ));
            }

            return result;
        }

        public IResponseBase ParseCart(IResultResponse response, IRequestParameter parameters)
        {
            Response<CartResponse> result = new Response<CartResponse>();

            try
            {
                // Check If Cart Is Empty
                var emptyCartCheck = _xDoc.Descendants(_ns + "div")
                                          .WhereAttributeEquals("class", "cart-empty")
                                          .Any();

                if (!emptyCartCheck)
                {
                    var mainContainer = _xDoc.Descendants(_ns + "form")
                                             .WhereAttributeEquals("id", "cart-items-form")
                                             .FirstOrNewXElement();

                    // Get Cart Items
                    var itemsContainer = mainContainer.Descendants(_ns + "tr")
                                                      .WhereAttributeEquals("class", "cart-row");

                    var items = ParseCartItems(itemsContainer);

                    //cperez:SPAY-43: MasterID is the LOT + Size. This is the equivalent to SKU
                    if (items != null && items.Any())
                        foreach (var cartItem in items)
                            cartItem.MasterProductId = cartItem.Sku;

                    // Get Price Summary
                    var summaryContainer = mainContainer.Descendants(_ns + "table")
                                                        .WhereAttributeEquals("class", "order-totals-table")
                                                        .FirstOrNewXElement();

                    var summary = ParseCartSummary(summaryContainer);

                    // Get Coupons
                    var couponsContainer = mainContainer.Descendants(_ns + "tr")
                                                        .WhereAttributeEquals("class", "rowcoupons");

                    var coupons = ParseCartCoupons(couponsContainer);

                    // Get Promo Message
                    var promoMessage = mainContainer.Descendants(_ns + "div")
                                                    .WhereAttributeEquals("class", "cart-promo")
                                                    .FirstOrNewXElement()
                                                    .ElementValue();

                    // Get DWQuery
                    var action = mainContainer.AttributeValue("action");
                    var begin = action.IndexOf("?dwcont=");
                    var dwQuery = string.Empty;

                    if (begin > -1)
                    {
                        dwQuery = action.Substring(begin, action.Length - begin);
                    }

                    var csrfToken = ParsingHelper.GetPasswordReset_CsrfToken(_xDoc);
                    if (!string.IsNullOrEmpty(csrfToken))
                    {
                        var strCsrfToken = csrfToken.Split('=').GetValue(1).ToString();
                        if (_session == null)
                        {
                            _session = new PaylessSession(_core);
                        }
                        var checkout = _session.GetCheckout();
                        checkout.CsrfToken = strCsrfToken;
                        _session.SetCheckout(checkout);
                    }

                    // Get Login Parameter Name
                    var dwLoginParam = _xDoc.Descendants(_ns + "form")
                                            .WhereAttributeEquals("id", "dwfrm_login")
                                            .Descendants(_ns + "input")
                                            .WhereAttributeContains("id", "dwfrm_login_username_")
                                            .FirstOrNewXElement()
                                            .AttributeValue("id");

                    // Get Errors
                    _errors.AddRange(ParseCartErrors(mainContainer));

                    // Allow to Checkout
                    var allowToCheckout = !_xDoc.Descendants(_ns + "button")
                                                .WhereAttributeEquals("name", "dwfrm_cart_checkoutCart")
                                                .WhereAttributeEquals("disabled", "disabled")
                                                .Any();

                    // Total Cart Item Count
                    var itemCount = 0;
                    items.ForEach(x => itemCount += x.Quantity);

                    

                    // Populate Result
                    result.resultset.Cart.CartItems = items;
                    result.resultset.Cart.Summary = summary;
                    result.resultset.Cart.Coupons = coupons;
                    result.resultset.Cart.PromoMessage = promoMessage;
                    result.resultset.Cart.DWQuery = dwQuery;
                    result.resultset.Cart.DWLoginParam = dwLoginParam;
                    result.resultset.Cart.AllowToCheckout = allowToCheckout;
                    result.resultset.Cart.CartItemCount = itemCount;
                    result.resultset.Cart.GoogleWalletInfo = ParsingHelper.GetCartGoogleWalletInfoFrom(response, result.resultset.Cart);

                }


                // Criteo Customer Id
                var criteoCustomerId = ParsingHelper.GetCriteoCustomerId(response);
                result.resultset.Cart.CriteoCustomerId = criteoCustomerId;

                // Get Promo Header
                var promoHeaderContainer =  _xDoc.Descendants(_ns + "div")
                                                    .WhereAttributeEquals("class", "header-banner")
                                                    .FirstOrNewXElement();

                promoHeaderContainer.Descendants(_ns + "script")
                                    .Remove();

                var promoHeader = string.Empty;
                if (Config.CartServicesParams.IsVisiblePromoHeader)
                    promoHeader = promoHeaderContainer.ElementValue();

                result.resultset.Cart.PromoHeader = promoHeader;
                result.resultset.TealiumDataBase = ParsingHelper.GetTealiumDataBase(_xDoc);
                result.resultset.TealiumDataExtended = ParsingHelper.GetTealiumDataExtended(_xDoc);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                   "MadServ.CartService.ParseCart",
                   ErrorSeverity.FollowUp,
                   ErrorType.Parsing
                   ));
            }

            
            return result;
        }        

        private List<CartItem> ParseCartItems(IEnumerable<XElement> container)
        {
            List<CartItem> result = new List<CartItem>();

            try
            {
                if (container != null)
                {
                    foreach (var xEl in container)
                    {
                        var src = xEl.Descendants(_ns + "td")
                                     .WhereAttributeEquals("class", "item-image")
                                     .FirstOrNewXElement()
                                     .Elements(_ns + "img")
                                     .FirstOrNewXElement()
                                     .AttributeValue("src");

                        var detailsContainer = xEl.Descendants(_ns + "div")
                                                  .WhereAttributeEquals("class", "product-list-item")
                                                  .FirstOrNewXElement();

                        var name = detailsContainer.Elements(_ns + "div")
                                                   .WhereAttributeEquals("class", "name")
                                                   .FirstOrNewXElement()
                                                   .ElementValue();

                        var href = detailsContainer.Elements(_ns + "div")
                                                   .WhereAttributeEquals("class", "name")
                                                   .FirstOrNewXElement()
                                                   .Elements(_ns + "a")
                                                   .FirstOrNewXElement()
                                                   .AttributeValue("href")
                                                   .Replace(Config.Urls.BaseUrl, string.Empty);

                        var productId = detailsContainer.Elements(_ns + "div")
                                                        .WhereAttributeEquals("class", "sku")
                                                        .FirstOrNewXElement()
                                                        .Elements(_ns + "span")
                                                        .WhereAttributeEquals("class", "value")
                                                        .FirstOrNewXElement()
                                                        .ElementValue();

                        var sku = ParsingHelper.GetSkuFromHref(href);

                        var color = detailsContainer.Descendants(_ns + "span")
                                                    .WhereAttributeEquals("class", "value Color")
                                                    .FirstOrNewXElement()
                                                    .ElementValue();

                        var size = detailsContainer.Descendants(_ns + "span")
                                                   .WhereAttributeEquals("class", "value Size")
                                                   .FirstOrNewXElement()
                                                   .ElementValue();

                        var width = detailsContainer.Descendants(_ns + "span")
                                                    .WhereAttributeEquals("class", "value Width")
                                                    .FirstOrNewXElement()
                                                    .ElementValue();

                        var design = detailsContainer.Descendants(_ns + "span")
                                                     .WhereAttributeEquals("class", "value Gift Card Design")
                                                     .FirstOrNewXElement()
                                                     .ElementValue();

                        var gcValue = detailsContainer.Descendants(_ns + "span")
                                                      .WhereAttributeEquals("class", "value Gift Card Value")
                                                      .FirstOrNewXElement()
                                                      .ElementValue();

                        var qtyStr = xEl.Descendants(_ns + "td")
                                        .WhereAttributeEquals("class", "item-quantity")
                                        .FirstOrNewXElement()
                                        .Elements(_ns + "input")
                                        .FirstOrNewXElement()
                                        .AttributeValue("value");
                        var qty = 0;
                        Int32.TryParse(qtyStr, out qty);

                        var itemPrice = xEl.Descendants(_ns + "span")
                                           .WhereAttributeEquals("class", "price-standard")
                                           .FirstOrNewXElement()
                                           .ElementValue();

                        var specialPrice = xEl.Descendants(_ns + "span")
                                              .WhereAttributeEquals("class", "price-sales")
                                              .FirstOrNewXElement()
                                              .ElementValue();

                        var listPrice = xEl.Descendants(_ns + "span")
                                           .WhereAttributeEquals("class", "price-unadjusted")
                                           .FirstOrNewXElement()
                                           .ElementValue();

                        if (string.IsNullOrEmpty(itemPrice))
                        {
                            itemPrice = specialPrice;
                            specialPrice = string.Empty;
                        }

                        var totalPrice = xEl.Descendants(_ns + "span")
                                            .WhereAttributeEquals("class", "price-total")
                                            .FirstOrNewXElement()
                                            .ElementValue();
                        var totalPriceLabel = string.Empty;
                        if (string.IsNullOrEmpty(totalPrice))
                        {
                            totalPrice = xEl.Descendants(_ns + "span")
                                            .WhereAttributeEquals("class", "price-adjusted-total")
                                            .FirstOrNewXElement()
                                            .ElementValue();
                            totalPriceLabel = xEl.Descendants(_ns + "div")
                                                 .WhereAttributeEquals("class", "promo-adjustment")
                                                 .Where(x => !string.IsNullOrEmpty(x.ElementValue()))
                                                 .FirstOrNewXElement()
                                                 .ElementValue();
                        }

                        var availabilityMessage = xEl.Descendants(_ns + "li")
                                                     .WhereAttributeEquals("class", "not-available")
                                                     .FirstOrNewXElement()
                                                     .ElementValue();

                        result.Add(new CartItem
                        {
                            Image = new Image { Src = src },
                            Name = name,
                            Href = href,
                            ProductId = productId,
                            Sku = sku,
                            Color = color,
                            Size = size,
                            Width = width,
                            Design = design,
                            GCValue = gcValue,
                            Quantity = qty,
                            ItemPrice = new Price(itemPrice) { Label = "Reg. " },
                            SpecialPrice = new Price(specialPrice) { Label = "Sale "},
                            ListPrice = new Price(listPrice) { Label = "List Price" },
                            TotalPrice = new Price(totalPrice) { Label = totalPriceLabel },
                            AvailabilityMessage = availabilityMessage
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                "MadServ.CartService.ParseCartItems",
                ErrorSeverity.FollowUp,
                ErrorType.Parsing
                ));
            }

            return result;
        }

        //private List<CouponCodeBase> ParseCartCoupons(IEnumerable<XElement> container)
        private List<CouponCodeBase> ParseCartCoupons(IEnumerable<XElement> container)
        {
            List<CouponCodeBase> result = new List<CouponCodeBase>();
            List<string> couponStatus = new List<string>();

            try
            {
                if (container != null)
                {
                    int ctr = 0;
                    foreach (var xEl in container)
                    {
                        var code = xEl.Descendants(_ns + "div")
                                      .WhereAttributeEquals("class", "cartcoupon clearfix")
                                      .FirstOrNewXElement()
                                      .Elements(_ns + "span")
                                      .WhereAttributeEquals("class", "value")
                                      .FirstOrNewXElement()
                                      .ElementValue();

                        var discountsContainer = xEl.Descendants(_ns + "div")
                                                    .WhereAttributeEquals("class", "discount");

                        if (discountsContainer.Count() > 0)
                        {
                            foreach (var discount in discountsContainer)
                            {

                                var message = discount.Elements(_ns + "span")
                                                      .WhereAttributeEquals("class", "label")
                                                      .FirstOrNewXElement()
                                                      .ElementValue();

                                var amount = discount.Elements(_ns + "span")
                                                     .WhereAttributeEquals("class", "value")
                                                     .FirstOrNewXElement()
                                                     .ElementValue();

                                result.Add(new CouponCodeBase
                                {
                                    Code = code,
                                    Message = message,
                                    CouponValue = new Price(amount)
                                });
                            }
                        }
                        else
                        {
                            result.Add(new CouponCodeBase
                            {
                                Code = code,
                                Message = "Not Applied",
                               // Message = "This promo code does not apply to any items in your cart.",
                                CouponValue = new Price()
                            });
                        }


                        var couponStatusContainer = xEl.Descendants(_ns + "td")
                                                    .WhereAttributeEquals("class", "coupon-actions");

                        
                        if (couponStatusContainer.Count() > 0)
                        {
                            
                            foreach (var statusMsg in couponStatusContainer)
                            {

                                //var message = statusMsg.Elements(_ns + "span")
                                //                      .WhereAttributeEquals("class", "label")
                                //                      .FirstOrNewXElement()
                                //                      .ElementValue();

                                var status =   statusMsg.Elements(_ns + "span")
                                              .WhereAttributeEquals("class", "coupon-status")
                                              .FirstOrNewXElement()
                                              .ElementValue();

                                if (!string.IsNullOrWhiteSpace(status))
                                {
                                    if (status.ToUpper().Contains("NOT APPLIED"))
                                    {

                                        //result.RemoveAt(ctr);
                                    }
                                    else
                                    {
                                        result[ctr].Message = result[ctr].Message + "  (" + status + ")";
                                    }
                                    
                                    ctr++;

                                }
                                    

                            }
                        }




                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                "MadServ.CartService.ParseCartCoupons",
                ErrorSeverity.FollowUp,
                ErrorType.Parsing
                ));
            }

            return result;
        }

        private CartSummaryBase ParseCartSummary(XElement container)
        {
            CartSummaryBase result = new CartSummaryBase();

            try
            {
                var total = container.Descendants(_ns + "tr")
                                     .WhereAttributeEquals("class", "order-total")
                                     .FirstOrNewXElement()
                                     .ElementValue();

                var subTotal = container.Descendants(_ns + "tr")
                                        .WhereAttributeEquals("class", "order-subtotal")
                                        .FirstOrNewXElement()
                                        .ElementValue();

                var discount = container.Descendants(_ns + "tr")
                                        .WhereAttributeEquals("class", "order-discount discount")
                                        .FirstOrNewXElement()
                                        .ElementValue();

                var shipping = container.Descendants(_ns + "tr")
                                        .WhereAttributeEquals("class", "order-shipping")
                                        .FirstOrNewXElement()
                                        .ElementValue();

                var shippingDiscount = container.Descendants(_ns + "tr")
                                                .WhereAttributeEquals("class", "order-shipping-discount discount")
                                                .FirstOrNewXElement()
                                                .ElementValue();

                var tax = container.Descendants(_ns + "tr")
                                   .WhereAttributeEquals("class", "order-sales-tax")
                                   .FirstOrNewXElement()
                                   .ElementValue();

                result.Total = new Price(total) { Label = "Estimated Total" };
                
                result.Costs = new List<Price>();

                result.Costs.Add(new Price(subTotal) { Label = "Subtotal" });
                
                if (!string.IsNullOrEmpty(discount))
                    result.Costs.Add(new Price(discount) { Label = "Order Discount" });
                
                if (!shipping.Contains("N/A"))
                    result.Costs.Add(new Price(shipping) { Label = "Shipping" });

                if (!string.IsNullOrEmpty(shippingDiscount))
                    result.Costs.Add(new Price(shippingDiscount) { Label = "Shipping Discount" });
                
                if (!tax.Contains("N/A"))
                    result.Costs.Add(new Price(tax) { Label = "Sales Tax" });
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                "MadServ.CartService.ParseCartSummary",
                ErrorSeverity.FollowUp,
                ErrorType.Parsing
                ));
            }

            return result;
        }

        private List<SiteError> ParseCartErrors(XElement container)
        {
            List<SiteError> result = new List<SiteError>();

            try
            {
                result = container.Descendants(_ns + "div")
                                  .Where(z => z.AttributeValue("class") == "error" || z.AttributeValue("class") == "error-message")
                                  .Select(x => new SiteError
                                      {
                                          Message = new ErrorMessage(x.ElementValue(), string.Empty),
                                          Severity = ErrorSeverity.UserActionRequired,
                                          Type = ErrorType.UserActionRequired
                                      })
                                  .ToList();
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                "MadServ.CartService.ParseErrors",
                ErrorSeverity.FollowUp,
                ErrorType.Parsing
                ));
            }

            return result;
        }

        private bool IsLoggedIn()
        {
            var result = false;

            try
            {
                result = _xDoc.Descendants(_ns + "ul")
                              .WhereAttributeEquals("class", "menu-utility-user")
                              .FirstOrNewXElement()
                              .Descendants(_ns + "a")
                              .WhereAttributeContains("title", "Logout")
                              .Any();
            }
            catch
            {
            }

            return result;
        }

        public void SaveCart(Cart cart)
        {
            var checkout = _session.GetCheckout();

            checkout.Cart = cart;
            checkout.IsLoggedIn = IsLoggedIn();

            _session.SetCheckout(checkout);
        }

        public void SaveCartItemCount(Cart cart)
        {
            var checkout = _session.GetCheckout();

            checkout.Cart.CartItemCount = cart.CartItemCount;

            _session.SetCheckout(checkout);
        }
    }
}
