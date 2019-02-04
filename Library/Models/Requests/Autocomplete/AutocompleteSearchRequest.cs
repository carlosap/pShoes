using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "AutocompleteSearchRequest")]
    public class AutocompleteSearchRequest : IRequestParameter
    {
        public string SearchToken { get; set; }
    }
}
