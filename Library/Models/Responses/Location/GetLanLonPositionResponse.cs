using MadServ.Core.Interfaces;
using System;
using System.ComponentModel.Composition;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "GetLanLonPositionResponse")]
    public class GetLanLonPositionResponse : IResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

