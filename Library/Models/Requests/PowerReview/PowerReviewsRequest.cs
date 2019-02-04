using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "PowerReviewsRequest")]
    public class PowerReviewsRequest : IRequestParameter
    {
        public string ProductId { get; set; }
        public string Sort { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

    }
}


