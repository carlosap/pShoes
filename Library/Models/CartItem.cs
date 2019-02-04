using System;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class CartItem : CartItemBase
    {
        public string Sku { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Width { get; set; }
        public string Design { get; set; }
        public string GCValue { get; set; }
        public Price SpecialPrice { get; set; }
        public Price ListPrice { get; set; }
        public Price CouponDiscount { get; set; }
        public bool IsInStock { get; set; }
        public string Href { get; set; }
        public string AvailabilityMessage { get; set; }
        public string MasterProductId { get; set; }

        public CartItem()
        {
            IsInStock = true;
        }
    }
}
