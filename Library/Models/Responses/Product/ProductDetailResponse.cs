using System;
using System.ComponentModel.Composition;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;
using MadServ.Core.Models.Responses;
using MadServ.Core.Models;
using System.Collections.Generic;
using Library.Helpers;
using Library.Models.ProductVariant;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "ProductDetailResponse")]
    public class ProductDetailResponse : ProductDetailResponseBase
    {
        new public Product Product { get; set; }
        new public IList<RecommendedProduct> RecommendedProducts { get; set; }
        new public ProductVariants ProductVariants { get; set; } 
        public string RequestedColor { get; set; }

        public ProductDetailResponse()
        {
            Product = new Product();
        }

        public ProductDetailResponse(DWProduct product, string color, ICore core, List<SiteError> errors)
        {
            RequestedColor = color;
            Product = new Product(product, color, core, errors);
        }
    }
}
