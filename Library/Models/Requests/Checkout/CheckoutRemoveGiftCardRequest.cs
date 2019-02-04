using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutRemoveGiftCardRequest")]
    public class CheckoutRemoveGiftCardRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }
        public string RemoveHref { get; set; }

        public CheckoutRemoveGiftCardRequest()
        {
            CheckoutResponse = new CheckoutResponse();
            RemoveHref = string.Empty;
        }
    }
}

