using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "InitRequest")]
    public class InitRequest : IRequestParameter
    {
        public string d { get; set; }
        public string SourceCode { get; set; }
        public string CampaignId  { get; set; }
    }
}
