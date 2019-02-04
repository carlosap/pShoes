using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "AccountOrderDetailResponse")]
    public class AccountOrderDetailResponse : AccountOrderDetailResponseBase
    {
        public string OrderNumber { get; set; }
        public string OrderedOn { get; set; }
        public string Status { get; set; }
        public string TrackingId { get; set; }
        public List<CartItem> Items { get; set; }
        public AddressBase ShippingInfo { get; set; }
        public List<string> BillingInfo { get; set; }
        public List<string> PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingStatus { get; set; }
        public CartSummaryBase Summary { get; set; }

        public AccountOrderDetailResponse()
        {
            Items = new List<CartItem>();
            ShippingInfo = new AddressBase();
            BillingInfo = new List<string>();
            PaymentMethod = new List<string>();
            Summary = new CartSummaryBase();
        }
    }
}
