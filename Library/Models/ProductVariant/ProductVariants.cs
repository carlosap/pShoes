using System.Collections.Generic;
namespace Library.Models.ProductVariant
{
    public class ProductVariants
    {
        public string _v { get; set; }
        public int count { get; set; }
        public List<Datum> data { get; set; }
        public int total { get; set; }
        public ProductVariants()
        {
            data = new List<Datum>();

        }
    }
}
