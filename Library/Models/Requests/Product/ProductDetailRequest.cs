using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "ProductDetailRequest")]
    public class ProductDetailRequest : IRequestParameter
    {
        public string Id { get; set; }
        public string Color { get; set; }
    }
}
