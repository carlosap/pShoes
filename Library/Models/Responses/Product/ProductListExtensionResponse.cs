using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using Library.Helpers;
using Library.Models.ProductVariant;
using MadServ.Core.Interfaces;
using Newtonsoft.Json;
using Library.Extensions;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "ProductListExtensionResponse")]
    public class ProductListExtensionResponse : IResponse
    {
        public Dictionary<string, ProductListItemExtension> ProductIdToExtension { get; set; }
        public ProductVariants ProductVariants { get; set; }
        public ProductListExtensionResponse()
        {
            ProductIdToExtension = new Dictionary<string, ProductListItemExtension>();
        }

        private DWImage GetSwatchImage(DWProduct product, string color)
        {
            //This is an alternative way in case linq failed -by Carlos SPAY-404
            //var swatchImages = new List<DWImage>();
            //foreach (var imageViewType in product.ImageGroups)
            //{
            //    if (imageViewType.ViewType.Equals("swatch"))
            //    {
            //        foreach (var image in imageViewType.Images)
            //        {
            //            if (image.Link.ToLower().Contains(color.Replace("_", "").ToLower()))
            //                return new DWImage() { Link = image.Link, Alt = image.Alt, Title = image.Title };
            //        }
            //    }
            //}

            var swatchImages = product.ImageGroups.Where(z => z.ViewType == "swatch" && z.VariationAttributes.Any(e => e.values.Any((v => v.value == color))))
                                                                .Select(z => z.Images) 
                                                                .FirstOrDefault();

            if (swatchImages != null)
            {
                foreach (var image in swatchImages)
                {
                    if (image.Link.ToLower().Contains(color.Replace("_", "").ToLower()))
                        return new DWImage() { Link = image.Link, Alt = image.Alt, Title = image.Title };
                }
            }
            return null;
        }

        private DWImage GetImagesForColor(DWProduct product, string color)
        {
            var images = product.ImageGroups.Where(z => z.ViewType == "hi-res" && z.VariationAttributes.Any(e => e.values.Any((v => v.value == color))))
                                                                .Select(z => z.Images)
                                                                .FirstOrDefault();
            if (images != null)
                return new DWImage() { Alt = images[0].Alt, Title = images[0].Title, Link = images[0].Link.Replace("/aapr_prd/", "/sits_pod15/dw/image/v2/AAPR_PRD/") + "?sw=200" };
            else
                return null;
        }

        public ProductListExtensionResponse(ProductResult productResult, List<string> colors, string htmlRaw = ""): this()
        {
            colors.Reverse();
            foreach (var product in productResult.Products)
            {
                var productId = product.Id;
                if (!ProductIdToExtension.ContainsKey(productId))
                { 
                    var extension = new ProductListItemExtension();

                    var availableVariations = product.VariationAttributes;
                    availableVariations.RemoveAll(a => a.Values == null);

                    List<VariationAttribute> outVA = new List<VariationAttribute>();
                    foreach (VariationAttribute va in availableVariations)
                    {
                        VariationAttribute attr = new VariationAttribute();
                        attr.Id = va.Id;
                        attr.Name = va.Name;
                        foreach (VariationAttributeValue innerVa in va.Values.ToList())
                        {
                            if (va.Id == "color")
                            {
                                attr.Values.Add(new VariationAttributeValue()
                                {                                     
                                    Image = GetImagesForColor(product, innerVa.Value),
                                    Swatch = GetSwatchImage(product, innerVa.Value),
                                    Name = innerVa.Name,
                                    Value = innerVa.Value.ToLower(),
                                    IsOrderable = innerVa.IsOrderable
                                });                                
                            }

                        }
                        outVA.Add(attr);
                    }                    

                    var colorVariations = availableVariations.Where(a => a.Id == "color").FirstOrDefault() ?? new VariationAttribute();
                    var applicableImageGroups = product.ImageGroups.Where(z => colorVariations.Values.Any(x => z.VariationAttributes.Select(e => e.values.Where(v => v.value.Contains(x.Value)).FirstOrDefault()).Any()));
                      


                    DWImage image = null;
                    var colorImages = applicableImageGroups.Where(z => z.ViewType == "hi-res"
                                                                    && colors.Find(x => z.VariationAttributes.Select(e => e.values.Where(v => v.value.Contains(x)).FirstOrDefault()).Any()) != null
                                                                    && z.Images.Any())
                                                            .Select(z => z.Images)
                                                            .FirstOrDefault();

                    if (colorImages != null)
                    {
                        image = colorImages.FirstOrDefault();
                    }
                    else
                    {
                        var genericImages = (applicableImageGroups.Count() > 0 ? applicableImageGroups : product.ImageGroups).Where(z => z.ViewType == "hi-res")
                                                                                                                                .Select(z => z.Images)
                                                                                                                                .FirstOrDefault();

                        if (genericImages != null)
                        {
                            image = genericImages.FirstOrDefault();
                        }
                    }

                    if (image != null)
                    {
                        extension.Image.Src = APIHelper.GetOptimizedImageSrc(image.Link);
                        extension.Image.Title = image.Title;
                    }
                    extension.Brand = product.Brand;
                    extension.Rating = product.AverageRating;
                    extension.IsAvailableInMultipleColors = colorVariations.Values.Count > 1;
                    extension.AvailableVariations = outVA;
                    extension.ProductFlags = product.ProductFlags;
                    ProductIdToExtension.Add(productId, extension);
                    extension.Callout = product.Promotions.Where(promo => promo.Callout != null).Select(a => a.Callout).ToList();
                   
                }
                SetProductBrands(productResult.Products, htmlRaw);
            }
        }

        private void SetProductBrands(List<DWProduct> products, string rawResponse)
        {
            if (!string.IsNullOrEmpty(rawResponse))
            {
                ProductVariants = JsonConvert.DeserializeObject<ProductVariants>(rawResponse);
            }
            foreach (var product in products)
            {
                var productId = product.Id;
                var isBranded = ProductVariants.data.FirstOrDefault(items => items.id.Equals(productId));
                if (isBranded != null)
                {
                    if (string.IsNullOrEmpty(product.Brand))
                    {
                        product.Brand = isBranded.c_displayBrand.AddSpaceAfterUpperCase(true);
                    }
                }
            }
        }

        public static ProductResult GetProductResult(DWProduct product)
        {
            var productResult = new ProductResult();
            productResult.Products.Add(product);
            return productResult;
        }
    }
}
