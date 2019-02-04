using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using Library.Models.PowerReview;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "PowerReviewSnapshotResponse")]
    public class PowerReviewSnapshotResponse : IResponse
    {
        public Snapshot Snapshot { get; set; }
        public PowerReviewSnapshotResponse()
        {
            Snapshot = new Snapshot();
        }

    }
}