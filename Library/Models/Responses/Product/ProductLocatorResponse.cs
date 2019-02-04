using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "ProductLocatorResponse")]
    public class ProductLocatorResponse : IResponse
    {
        public List<Store> Locations { get; set; }
        public ProductLocatorResponse()
        {
            Locations = new List<Store>();
        }
    }
}
