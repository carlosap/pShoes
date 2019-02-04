using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "FindMyShoeRequest")]
    public class FindMyShoeRequest : IRequestParameter
    {
        public string Href { get; set; }
    }
}
