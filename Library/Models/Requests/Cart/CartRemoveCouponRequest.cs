using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartRemoveCouponRequest")]
    public class CartRemoveCouponRequest : IRequestParameter
    {
        public int ItemIndex { get; set; }
    }
}
