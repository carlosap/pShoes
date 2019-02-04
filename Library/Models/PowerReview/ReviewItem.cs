namespace Library.Models.PowerReview
{
    public class ReviewItem
    {
        public string reviewId { get; set; }
        public string merchantGroupId { get; set; }
        public string merchantId { get; set; }
        public int Rating { get; set; }
        public string Headline { get; set; }
        public string Comments { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ComfortMsg { get; set; }
        public string DurabilityMsg { get; set; }
        public string SizingMsg { get; set; }
        public string Bottomline { get; set; }
        public string DateCreated { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        public int TotalHelpfulVote { get; set; }

    }
}
