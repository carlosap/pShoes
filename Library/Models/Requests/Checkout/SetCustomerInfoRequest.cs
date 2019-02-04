using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests.SetCustomerInfo
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "SetCustomerInfoRequest")]
    public class SetCustomerInfoRequest : IRequestParameter
    {
        public string Email { get; set; }

        public SetCustomerInfoRequest()
        {
        }
    }
}
