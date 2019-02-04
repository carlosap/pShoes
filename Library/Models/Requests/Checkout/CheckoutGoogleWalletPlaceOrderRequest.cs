using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutGoogleWalletPlaceOrderRequest")]
    public class CheckoutGoogleWalletPlaceOrderRequest : IRequestParameter
    {
        public string Jwt { get; set; }
        public string Pan { get; set; }
        public string Cvn { get; set; }
    }
}
