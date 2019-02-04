using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "ProductLocatorRequest")]
    public class ProductLocatorRequest : IRequestParameter
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Zip { get; set; }

        public int NumStoresToReturn { get; set; }
        public int SearchRadius { get; set; }
        public string ProductSku { get; set; }

        internal List<Store> Stores { get; set; }

        public ProductLocatorRequest()
        {
            NumStoresToReturn = 3;
            SearchRadius = 30;
            Stores = new List<Store>();
        }
    }
}
