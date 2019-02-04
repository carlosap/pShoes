using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutBillingRequest")]
    public class CheckoutBillingRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }
        public bool IsApplyGiftCard { get; set; }

        public CheckoutBillingRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
