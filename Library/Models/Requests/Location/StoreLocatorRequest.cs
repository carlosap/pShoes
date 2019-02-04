using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "StoreLocatorRequest")]
    public class StoreLocatorRequest : IRequestParameter
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public string Zip { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StoreHours { get; set; }
        public int Radius { get; set; }

        public StoreLocatorRequest()
        {
            Radius = Config.Params.DefaultRadius;
        }
    }
}
