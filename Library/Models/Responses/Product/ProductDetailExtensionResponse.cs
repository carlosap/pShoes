using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;
using Newtonsoft.Json;
using Library.Models.ProductVariant;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "ProductDetailExtensionResponse")]
    public class ProductDetailExtensionResponse : IResponse
    {
        public Dictionary<string, ProductDetailItemExtension> VariantIdToExtension { get; set; }
        public ProductVariants ProductVariants { get; set; }
        public ProductDetailExtensionResponse()
        {
            VariantIdToExtension = new Dictionary<string, ProductDetailItemExtension>();
            ProductVariants = new ProductVariants();
        }
        public ProductDetailExtensionResponse(ProductResult productResult, string htmlRaw="") : this()
        {
            ProductVariants = new ProductVariants();
            SetProducts(productResult,htmlRaw);
        }
        private void SetProducts(ProductResult productResult,string rawResponse)
        {
            if (!string.IsNullOrEmpty(rawResponse))
            {
                rawResponse =
                    rawResponse.Replace("pricebook-saleprice", "pricebooksaleprice")
                        .Replace("pricebook-listprice", "pricebooklistprice")
                        .Replace("pricebook-clearanceprice", "pricebookclearanceprice");
                ProductVariants = JsonConvert.DeserializeObject<ProductVariants>(rawResponse);
            }
            foreach (var product in productResult.Products)
            {
                var productId = product.Id;
                if (!VariantIdToExtension.ContainsKey(productId))
                {
                    var extension = new ProductDetailItemExtension
                    {
                        SizeSegment = product.SizeSegment,
                        ExclusiveMsg = GetExclusiveMsg(productId),
                        Brand = product.Brand,
                        PriceRange = GetPriceRanges(productId)
                    };
                    VariantIdToExtension.Add(productId, extension);
                }
            }
        }
        private ProductVariant.Prices GetPriceRanges(string productId)
        {
            var prices = new ProductVariant.Prices();
            if (ProductVariants == null || ProductVariants.count <= 0)
            {
                return prices;
            }
            try
            {
                foreach (var item in ProductVariants.data)
                {
                    if (productId != null && !item.id.Equals(productId))
                    {
                        continue;
                    }
                    prices = item.prices;
                }
            }
            catch (Exception)
            {
                //protecting unknown flags
            }
            return prices;
        }

        private string GetExclusiveMsg(string productId)
        {
            var result = string.Empty;
            if (ProductVariants == null || ProductVariants.count <= 0)
            {
                return result;
            }
            try
            {
                foreach (var item in ProductVariants.data)
                {
                    if (!item.id.Equals(productId) || (item.c_productFlags == null) || item.c_productFlags.Length <= 0)
                    {
                        continue;
                    }
                    foreach (var flag in item.c_productFlags)
                    {
                        if (!Config.CallOutLookup.ContainsKey(flag))
                        {
                            continue;
                        }
                        var flagDefinition = Config.CallOutLookup[flag];
                        switch (flagDefinition)
                        {
                            case "Online Exclusive Available":
                            case "Online Exclusive":
                                return flagDefinition;
                            default:
                                result = string.Empty;
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //protecting unknown flags
                //No possible to support all DW
                //Flags. Return "Blank" on Error.
                //Meets Business requiremens. 
                result = string.Empty;
            }
            return result;
        }
        public static ProductResult GetProductResult(DWProduct product)
        {
            var productResult = new ProductResult();
            productResult.Products.Add(product);
            return productResult;
        }
    }
}
