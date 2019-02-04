using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "ResetPasswordFormRequest")]
    public class ResetPasswordFormRequest : IRequestParameter
    {
        public string Token { get; set; }
    }
}
