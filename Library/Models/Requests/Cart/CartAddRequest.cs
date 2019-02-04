using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartAddRequest")]
    public class CartAddRequest : IRequestParameter
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }

        public CartAddRequest()
        {
            Quantity = 1;
        }
    }
}
