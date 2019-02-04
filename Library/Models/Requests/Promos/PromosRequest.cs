using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "PromosRequest")]
    public class PromosRequest : IRequestParameter
    {
        public string PromoName { get; set; }

    }
}


