namespace Library.Models.PowerReview
{
    public class FaceOff
    {
        public Negative negative { get; set; }
        public Positive positive { get; set; }
        public FaceOff()
        {
            negative = new Negative();
            positive = new Positive();
        }
    }
}

