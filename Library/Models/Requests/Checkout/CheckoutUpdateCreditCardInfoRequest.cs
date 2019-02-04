using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutUpdateCreditCardInfoRequest")]
    public class CheckoutUpdateCreditCardInfoRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }
        public string CardId { get; set; }

        public CheckoutUpdateCreditCardInfoRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
