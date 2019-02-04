using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using Library.Models.PowerReview;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "PowerReviewFaceOffResponse")]
    public class PowerReviewFaceOffResponse : IResponse
    {
        public FaceOff FaceOff { get; set; }
        public PowerReviewFaceOffResponse()
        {
            FaceOff = new FaceOff();
        }

    }
}