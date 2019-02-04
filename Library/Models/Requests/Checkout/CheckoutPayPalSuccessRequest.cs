using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutPayPalSuccessRequest")]
    public class CheckoutPayPalSuccessRequest : IRequestParameter
    {
        public string Token { get; set; }
        public string PayerId { get; set; }
        public string DWControl { get; set; }
    }
}
