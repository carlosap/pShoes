using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartApplyCouponRequest")]
    public class CartApplyCouponRequest : IRequestParameter
    {
        public List<CouponCodeBase> Coupons { get; set; }
        public string Email { get; set; }


        public CartApplyCouponRequest()
        {
            Coupons = new List<CouponCodeBase>();
        }
    }
}
