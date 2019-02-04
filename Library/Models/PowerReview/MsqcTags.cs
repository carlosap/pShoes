namespace Library.Models.PowerReview
{
    public class MsqcTags
    {
        public Durability Durability { get; set; }
        public Sizing Sizing { get; set; }
        public Comfort Comfort { get; set; }
        public MsqcTags()
        {
            Durability = new Durability();
            Sizing = new Sizing();
            Comfort = new Comfort();
            Durability.TotalEntries = 0;
            Comfort.TotalEntries = 0;
            Sizing.TotalEntries = 0;
        }
    }
}
