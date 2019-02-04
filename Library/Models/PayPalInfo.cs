using System;

namespace Library.Models
{
    [Serializable]
    public class PayPalInfo
    {
        public string PayPalRedirectUrl { get; set; }
        public bool IsBillingInfoStep { get; set; }
        public bool IsSuccess { get; set; }
    }
}
