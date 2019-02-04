using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests.SetAddress
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "SetAddressRequest")]
    public class SetAddressRequest : IRequestParameter
    {
        public Address Address { get; set; }
        public string Phone { get; set; }

        public bool IsBillingAddress { get; set; }

        public SetAddressRequest()
        {
            Address = new Address();
        }
    }
}
