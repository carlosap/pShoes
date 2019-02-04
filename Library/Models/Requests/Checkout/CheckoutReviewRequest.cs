using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutReviewRequest")]
    public class CheckoutReviewRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }

        public CheckoutReviewRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
