using System;
using MadServ.Core.Models;
namespace Library.Models
{
    [Serializable]
    public class Coupon
    {
        public string status { get; set; }
        public string message { get; set; }
        public string sucess { get; set; }
        public string couponType { get; set; }
        public string loyaltyRequired { get; set; }

        public string Code { get; set; }
        public Price CouponValue { get; set; }
        public string Message { get; set; }


        public Coupon()
        {


        }
    }
}
