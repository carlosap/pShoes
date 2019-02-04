using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using System.Collections.Generic;
using Library.Models.PowerReview;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "PowerReviewsResponse")]
    public class PowerReviewsResponse : IResponse
    {
        public string SortBy { get; set; }
        public List<ReviewItem> Reviews { get; set; }
        public Pagination Pagination { get; set; }
        public PowerReviewsResponse()
        {
            Reviews = new List<ReviewItem>();
            Pagination = new Pagination();
        }
    }
}