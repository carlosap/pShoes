using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartDetailRequest")]
    public class CartDetailRequest : IRequestParameter
    {
        public string SourceCode { get; set; }
        public string CampaignId { get; set; }
    }
}
