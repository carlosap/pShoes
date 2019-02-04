using System.ComponentModel.Composition;
using Library.Models.Responses;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "ForgotPasswordRequest")]
    public class ForgotPasswordRequest : IRequestParameter
    {
        public string UserName { get; set; }
        internal ForgotPasswordFormResponse Form { get; set; }

        public ForgotPasswordRequest()
        {
            Form = new ForgotPasswordFormResponse();
        }
    }
}
