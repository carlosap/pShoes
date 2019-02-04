using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutShippingRequest")]
    public class CheckoutShippingRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }

        public CheckoutShippingRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
