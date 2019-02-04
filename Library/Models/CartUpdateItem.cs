using System;

namespace Library.Models
{
    [Serializable]
    public class CartUpdateItem
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public int Index { get; set; }
    }
}
