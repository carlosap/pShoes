using System;
using System.Collections.Generic;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;

namespace Library.Models
{
    [Serializable]
    public class CheckoutResponse : CheckoutResponseBase
    {
        public new Address BillingInfo { get; set; }
        public new Address ShippingInfo { get; set; }
        public GiftCard GiftCard { get; set; }
        public List<GiftCard> AppliedGiftCards { get; set; }
        public new Cart Cart { get; set; }
        public new ShippingOptions ShippingOptions { get; set; }
        public ReviewInfo ReviewInfo { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string LoyaltyEmail { get; set; }
        public new List<SavedAddressOption> SavedAddresses { get; set; }
        public List<Option> SavedCards { get; set; }
        public bool IsAddToAddressBook { get; set; }
        public bool IsSaveThisCard { get; set; }
        public bool IsAddToEmailList { get; set; }
        public bool EnrollLoyalty { get; set; }
        public string ApplyCouponMessage { get; set; }
        public List<Option> PaymentMethods { get; set; }
        public PayPalInfo PayPalInfo { get; set; }

        public string DWSecureKey { get; set; }
        public string CsrfToken { get; set; }
        public string CaptchaChalange { get; set; }
        public string CaptchaResponse { get; set; }
        public string CaptchaHref { get; set; }
        public string DWCaptchaSecureKey { get; set; }
        public bool IsNoPaymentNeeded { get; set; }

        public dynamic TealiumDataBase { get; set; }
        public dynamic TealiumDataExtended { get; set; }

        public CheckoutResponse()
        {
            GiftCard = new GiftCard();
            AppliedGiftCards = new List<GiftCard>();
            BillingInfo = new Address();
            ShippingInfo = new Address();
            Cart = new Cart();
            SavedAddresses = new List<SavedAddressOption>();
            ShippingOptions = new ShippingOptions();
            SavedCards = new List<Option>();
            ReviewInfo = new ReviewInfo();
            PayPalInfo = new PayPalInfo();
            PaymentMethods = new List<Option>();
        }

        public CheckoutResponse(Basket apiBasket, Cart cart, CheckoutResponse checkout) : this()
        {
            Cart = new Cart(apiBasket);
            Cart.CartItems = cart.CartItems;
            Cart.SelectedShippingOption = cart.SelectedShippingOption;
            Cart.ShippingMethods = cart.ShippingMethods;

            Email = checkout.Email;
            Phone = checkout.Phone;
            SavedAddresses = checkout.SavedAddresses;
            IsLoggedIn = checkout.IsLoggedIn;

            if (apiBasket.CustomerInfo != null)
            {
                Email = apiBasket.CustomerInfo.Email;
            }

            if (apiBasket.Shipments.Any())
            {
                var firstShipment = apiBasket.Shipments.First();
                ShippingInfo = new Address(firstShipment.ShippingAddress);

                if (firstShipment.ShippingMethod != null)
                {
                    //ReviewShippingMethod = firstShipment.ShippingMethod.Name;
                }
            }

            if (apiBasket.BillingAddress != null)
            {
                BillingInfo = new Address(apiBasket.BillingAddress);
            }

            PaymentInfo = checkout.PaymentInfo;
        }
    }
}
