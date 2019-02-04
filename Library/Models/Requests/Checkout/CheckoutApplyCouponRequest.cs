using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutApplyCouponRequest")]
    public class CheckoutApplyCouponRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }
        public string CouponCode { get; set; }

        public CheckoutApplyCouponRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
