using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutPayPalCancelRequest")]
    public class CheckoutPayPalCancelRequest : IRequestParameter
    {
        public string Token { get; set; }
    }
}
