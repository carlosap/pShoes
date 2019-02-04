using System;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class GiftCard
    {
        public string GiftCardNumber { get; set; }
        public string Message { get; set; }
        public Price Discount { get; set; }
        public string RemoveHref { get; set; }
        public string GiftCardPinNumber { get; set; }
    }
}
