using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartUpdateRequest")]
    public class CartUpdateRequest : IRequestParameter
    {
        public List<CartUpdateItem> Items { get; set; }

        public CartUpdateRequest()
        {
            Items = new List<CartUpdateItem>();
        }
    }
}
