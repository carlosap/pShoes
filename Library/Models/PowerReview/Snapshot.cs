namespace Library.Models.PowerReview
{
    public class Snapshot
    {
        public Rating_Histogram rating_histogram { get; set; }
        public Histogram_Percent histogram_percent { get; set; }
        public int percent_recommended { get; set; }
        public string average_rating { get; set; }
        public int num_reviews { get; set; }

        public Snapshot()
        {
            histogram_percent = new Histogram_Percent();
        }
    }
}