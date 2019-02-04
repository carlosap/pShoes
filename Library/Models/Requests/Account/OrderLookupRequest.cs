using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "OrderLookupRequest")]
    public class OrderLookupRequest : IRequestParameter
    {
        public string OrderId { get; set; }
        public string Email { get; set; }
        internal string QueryString { get; set; }
    }
}
