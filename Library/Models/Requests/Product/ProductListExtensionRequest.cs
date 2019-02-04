using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "ProductListExtensionRequest")]
    public class ProductListExtensionRequest : IRequestParameter
    {
        public string ProductsIds { get; set; }
        public List<string> Colors { get; set; }
        public List<ProductListItem> Products { get; set; }

        public ProductListExtensionRequest()
        {
            Colors = new List<string>();
            Products = new List<ProductListItem>();
        }
    }
}
