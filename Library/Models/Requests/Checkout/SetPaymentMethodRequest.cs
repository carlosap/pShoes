using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests.SetPaymentMethod
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "SetPaymentMethodRequest")]
    public class SetPaymentMethodRequest : IRequestParameter
    {
        public string SelectedPaymentMethod { get; set; }

        public SetPaymentMethodRequest()
        {
        }
    }
}
