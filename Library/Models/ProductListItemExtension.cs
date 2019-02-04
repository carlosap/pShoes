using System;
using System.Collections.Generic;
using MadServ.Core.Models;
using Library.DemandWare.Models.DTOs;

namespace Library.Models
{
    [Serializable]
    public class ProductListItemExtension
    {
        public Image Image { get; set; }
        public List<VariationAttribute> AvailableVariations { get; set; }
        public List<string> ItemFeatures { get; set; }
        public string Brand { get; set; }
        public bool IsBOGO { get; set; }
        public bool IsSale { get; set; }
        public decimal Rating { get; set; }
        public bool IsAvailableInMultipleColors { get; set; }
        public List<string> ProductFlags { get; set; }
        public List<string> Callout { get; set; }

        public ProductListItemExtension()
        {
            Image = new Image
            {
                Src = "/assets/img/image_not_available.gif"
            };
            ItemFeatures = new List<string>();
        }
    }
}
