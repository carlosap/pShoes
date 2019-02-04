using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutUpdateSummaryRequest")]
    public class CheckoutUpdateSummaryRequest : IRequestParameter
    {
        public CheckoutResponse CheckoutResponse { get; set; }

        public CheckoutUpdateSummaryRequest()
        {
            CheckoutResponse = new CheckoutResponse();
        }
    }
}
