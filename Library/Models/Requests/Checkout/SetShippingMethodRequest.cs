using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests.SetShippingMethod
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "SetShippingMethodRequest")]
    public class SetShippingMethodRequest : IRequestParameter
    {
        public string SelectedShippingOption { get; set; }
        public string SelectedShippingMethod { get; set; }

        public SetShippingMethodRequest()
        {
        }
    }
}
