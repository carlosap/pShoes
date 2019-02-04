using System;
using System.Collections.Generic;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ReviewInfo
    {
        public List<string> Shipping { get; set; }
        public List<string> Billing { get; set; }
        public List<string> Payment { get; set; }

        public CartSummaryBase Summary { get; set; }
        public List<CartItem> CheckoutItems { get; set; }

        public bool IsGoogleWallet { get; set; }

        public ReviewInfo()
        {
            Shipping = new List<string>();
            Billing = new List<string>();
            Payment = new List<string>();

            Summary = new CartSummaryBase();
            CheckoutItems = new List<CartItem>();
        }
    }
}
