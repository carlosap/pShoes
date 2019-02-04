using System;
using System.Collections.Generic;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class Cart : CartBase
    {
        
        new public List<CartItem> CartItems { get; set; }
        public List<CartItemBase> UnavailableItems { get; set; }
        public string SelectedShippingOption { get; set; }
        public List<Option> ShippingMethods { get; set; }

        //public List<CouponCodeBase> Coupons { get; set; }
        public List<CouponCodeBase> Coupons { get; set; }
        public string PromoMessage { get; set; }
        public string PromoHeader { get; set; }
        public int CartItemCount { get; set; }
        public string DWQuery { get; set; }
        public string DWLoginParam { get; set; }

        public bool AllowToCheckout { get; set; }
        public GoogleWalletInfo GoogleWalletInfo { get; set; }
        public string CriteoCustomerId { get; set; }

        public Cart()
        {
            ShippingMethods = new List<Option>();
            CartItems = new List<CartItem>();
            UnavailableItems = new List<CartItemBase>();
            Summary.Costs = new List<Price>();
            Coupons = new List<CouponCodeBase>();
            CouponCode = null;
            GoogleWalletInfo = null;
        }

        public Cart(Basket apiBasket) : this()
        {
            if (apiBasket.Items.Any())
            {
                foreach (var item in apiBasket.Items)
                {
                    CartItemCount += (int)item.Quantity;
                    var cartItem = new CartItem
                    {
                        Name = item.Name,
                        ProductId = item.Id.ToString(),
                        Quantity = (int) item.Quantity,
                        ItemPrice = new Price(item.BasePrice.ToString()),
                        TotalPrice = new Price(item.AdjustedPrice.ToString())
                    };

                    if (item.PriceAdjustments.Any())
                    {
                        cartItem.ListPrice = new Price(item.Price.ToString());
                        cartItem.CouponDiscount = new Price(item.PriceAdjustments.FirstOrDefault().Discount.ToString())
                        {
                            Label = item.PriceAdjustments.FirstOrDefault().Description
                        };
                    }

                    CartItems.Add(cartItem);
                }

                // Summary
                Summary.Costs.Add(new Price(apiBasket.ProductSubTotal.ToString()) { Label = "Subtotal: " });

                if (apiBasket.PriceAdjustments.Any())
                {
                    var discount = 0.0M;
                    var description = string.Empty;
                    apiBasket.PriceAdjustments.ForEach(x => { discount += x.Discount; description += "<br>" + x.Description; });
                    Summary.Costs.Add(new Price(discount.ToString()) { Label = string.Format("Order Discount: {0}", description) });
                }
                if (apiBasket.ShippingTotal > 0)
                {
                    Summary.Costs.Add(new Price(apiBasket.ShippingTotal.ToString()) { Label = "Shipping: " });
                }

                Summary.Total = new Price(apiBasket.OrderTotal.ToString()) { Label = "Estimated Total: " };

                // Coupons
                apiBasket.Coupons.ForEach(x =>
                {
                    var message = string.Empty;

                    switch (x.StatusCode)
                    {
                        case CouponCodeStatusCodeEnum.applied:
                            message = Config.Constants.CouponAppliedMessage;
                            break;
                        case CouponCodeStatusCodeEnum.no_applicable_promotion:
                            message = Config.Constants.CouponNAMessage;
                            break;
                        default:
                            message = Config.Constants.CouponInvalidMessage;
                            break;
                    }

                    Coupons.Add(new CouponCodeBase
                    {

                        Code = x.Code,
                        CouponValue = new Price(),
                        Message = string.Format(message, x.Code)
                    });
                });
            }
        }

        public bool ExceedsMaxForGoogleWallet
        {
            get
            {
                return Summary.Total.Value > Config.GWMaxCart;
            }
        }
    }
}
