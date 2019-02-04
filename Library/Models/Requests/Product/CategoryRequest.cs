using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CategoryRequest")]
    public class CategoryRequest : IRequestParameter
    {
        public string Href { get; set; }
        public string d { get; set; }
    }
}
