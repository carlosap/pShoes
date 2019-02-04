using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "StoreLocatorResponse")]
    public class StoreLocatorResponse : IResponse
    {
        public List<Store> Locations { get; set; }
        public string UrlTarget { get; set; }
        public StoreLocatorResponse()
        {
            Locations = new List<Store>();
        }

        public StoreLocatorResponse(StoreResult storeResult)
            : this()
        {
            foreach (var store in storeResult.Stores)
            {
                if (!store.ShipToStore)
                {
                    Locations.Add(new Store(store));
                }
            }
        }
    }
}