using MadServ.Core.Models;
using System;

namespace Library.Models
{
    [Serializable]
    public class ProductImage : Image
    {
        public string SrcMed { get; set; }
        public string SrcLarge { get; set; }

        public ProductImage()
        {
    
        }
    }
}
