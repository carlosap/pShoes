using System.Collections.Generic;
namespace Library.Models.PowerReview
{
    public class PowerReview
    {
        public string SortBy { get; set; }
        public List<ReviewItem> Reviews { get; set; }
        public Snapshot Snapshot { get; set; }
        public MsqcTags MsqcTags { get; set; }
        public FaceOff FaceOff { get; set; }
        public Pagination Pagination { get; set; }
        public PowerReview()
        {
            SortBy = "desc";
            Reviews = new List<ReviewItem>();
            MsqcTags = new MsqcTags();
            FaceOff = new FaceOff();
            Pagination = new Pagination();
        }       
    }

}
