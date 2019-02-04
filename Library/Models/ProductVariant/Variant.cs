using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Library.Models.ProductVariant
{
    public class Variant
    {
        public int Id { get; set; }
        public List<string> ProductFlags { get; set; }
        public Variant()
        {
            ProductFlags = new List<string>();
        }
    }
}
