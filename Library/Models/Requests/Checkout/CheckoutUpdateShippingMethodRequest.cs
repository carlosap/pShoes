using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutUpdateShippingMethodsRequest")]
    public class CheckoutUpdateShippingMethodsRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }

        public CheckoutUpdateShippingMethodsRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
