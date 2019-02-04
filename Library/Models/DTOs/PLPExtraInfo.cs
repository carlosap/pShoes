namespace Library.Models
{
    public class PLPExtraInfo
    {
        public string name { get; set; }
        public string pid { get; set; }
        public string defaultColor { get; set; }
        public Prices prices { get; set; }
        public bool IsSale { get; set; }
        public bool IsBogo { get; set; }
        public bool IsClearance { get; set; }
    }

    public class Prices
    {
        public string standard { get; set; }
        public string sale { get; set; }
        public string rangelow { get; set; }
        public string rangehigh { get; set; }
    }
}
