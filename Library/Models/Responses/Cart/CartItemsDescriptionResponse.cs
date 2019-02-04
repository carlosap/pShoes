using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using Library.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "CartItemsDescriptionResponse")]
    public class CartItemsDescriptionResponse : IResponse
    {
        public List<CartItem> CartItems { get; set; }

        public CartItemsDescriptionResponse()
        {
            CartItems = new List<CartItem>();
        }

        public CartItemsDescriptionResponse(List<CartItem> cartItems, ProductResult productResult) : this()
        {
            CartItems = cartItems;

            foreach (var product in productResult.Products)
            {
                var matchingCartItem = CartItems.Find(x => x.ProductId == product.Id.ToString());
                if (matchingCartItem != null)
                {
                    var isGiftCard = product.Brand == "GIFT CARD";
                    var isOneSizeOnly = false;
                    var sizeVariations = product.VariationAttributes.Find(x => x.Id == "size" && x.Values.Any());
                    var colorVariations = product.VariationAttributes.Find(x => x.Id == "color" && x.Values.Any());

                    if (sizeVariations != null && sizeVariations.Values.Count.Equals(1))
                    {
                        isOneSizeOnly = sizeVariations.Values.First().Name == "One Size";
                    }

                    DWImage image = null;
                    var colorImages = product.ImageGroups.Where(z => z.ViewType == "hi-res"
                                                                   && z.VariationAttributes.Select(e => e.values.Where(v => v.value.Equals(product.Color)).FirstOrDefault()).Any()
                                                                   && z.Images.Any())
                                                          .Select(z => z.Images)
                                                          .FirstOrDefault();

                    if (colorImages != null)
                    {
                        image = colorImages.FirstOrDefault();
                    }
                    else
                    {
                        var genericImages = product.ImageGroups.Where(z => z.ViewType == "hi-res")
                                                                .Select(z => z.Images)
                                                                .FirstOrDefault();

                        if (genericImages != null)
                        {
                            image = genericImages.FirstOrDefault();
                        }
                    }

                    if (image != null)
                    {
                        matchingCartItem.Image = new Image { Src = APIHelper.GetOptimizedImageSrc(image.Link), Title = image.Title };
                    }
                    else
                    {
                        matchingCartItem.Image = new Image { Src = "/assets/img/image_not_available.gif" };
                    }

                    if (!string.IsNullOrEmpty(product.Size) && !isOneSizeOnly && !isGiftCard)
                    {
                        var decimalSize = 0.0M;
                        Decimal.TryParse(product.Size, out decimalSize);

                        if (decimalSize > 0)
                        {
                            matchingCartItem.Size = string.Format("{0:N1}", decimalSize / 10.0M);
                        }
                        else
                        {
                            matchingCartItem.Size = product.Size;
                        }
                    }
                    if (!isOneSizeOnly && !isGiftCard)
                    {
                        matchingCartItem.Width = Config.Params.USTextInfo.ToTitleCase(product.Width);
                    }
                    if (colorVariations != null)
                    {
                        var itemColorVariant = colorVariations.Values.Find(x => x.Value == product.Color);

                        if (itemColorVariant != null)
                        {
                            matchingCartItem.Color = itemColorVariant.Name;
                        }
                    }
                    
                    matchingCartItem.Description = product.Brand;
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
