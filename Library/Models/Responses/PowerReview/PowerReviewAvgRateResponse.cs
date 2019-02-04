using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using System.Collections.Generic;
using Library.Models.PowerReview;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "PowerReviewAvgRateResponse")]
    public class PowerReviewAvgRateResponse : IResponse
    {
        public List<AvgRate> AvgRates { get; set; }
        public PowerReviewAvgRateResponse()
        {
            AvgRates = new List<AvgRate>();
        }

    }
}