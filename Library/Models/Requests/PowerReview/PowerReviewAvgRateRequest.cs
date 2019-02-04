using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "PowerReviewAvgRateRequest")]
    public class PowerReviewAvgRateRequest : IRequestParameter
    {
        public string ProductId { get; set; }
    }
}


