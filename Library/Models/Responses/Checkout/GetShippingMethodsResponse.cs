using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "GetShippingMethodsResponse")]
    public class GetShippingMethodsResponse : IResponse
    {
        public List<Option> ShippingMethods { get; set; }
        public string ShipToStoreId { get; set; }

        public GetShippingMethodsResponse()
        {
            ShippingMethods = new List<Option>();
        }

        public GetShippingMethodsResponse(ShippingMethodResult apiResult) : this()
        {
            if (apiResult.ShippingMethods.Any())
            {
                foreach (var method in apiResult.ShippingMethods)
                {
                    if (method.CybersourceShippingId != null)
                    {
                        if (method.CybersourceShippingId != "pickup")
                        {
                            ShippingMethods.Add(new Option
                            {
                                Name = string.Format("{0} ({1}) - ${2:N2}", method.Name, method.Description, method.Price),
                                Value = method.Id
                            });
                        }
                        else
                        {
                            ShipToStoreId = method.Id;
                        }
                    }
                }

                if (ShippingMethods.Any())
                {
                    ShippingMethods.FirstOrDefault().IsSelected = true;
                }
            }
        }
    }
}
