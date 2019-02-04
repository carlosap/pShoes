using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartRemoveRequest")]
    public class CartRemoveRequest : IRequestParameter
    {
        public string Sku { get; set; }
        public int ItemIndex { get; set; }
    }
}
