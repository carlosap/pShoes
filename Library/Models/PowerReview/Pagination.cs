namespace Library.Models.PowerReview
{
    public class Pagination
    {
        public int DisplayFrom { get; set; }
        public int DisplayTo { get; set; }
        public int CurrentPage { get; set; }
        public string Sort { get; set; }
    }
}
