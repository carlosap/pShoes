using System.ComponentModel.Composition;
using Library.Models.Responses;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "LoginRequest")]
    public class LoginRequest : IRequestParameter
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public LoginFormResponse Form { get; set; }
    }
}
