using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using Library.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Helpers;
using Enums;
using Library.PowerReview;
using Library.Models.Requests;
using Library.IRequests;
using Library.Models.Responses;
using Library.Models.PowerReview;

namespace Library.Models
{
    [Serializable]
    public class Product : ProductBase
    {
        public string AdditionalInfo { get; set; }
        public string CareInstructions { get; set; }
        public List<ColorOption> Colors { get; set; }
        public List<DesignOption> Designs { get; set; }
        public Library.Models.PowerReview.PowerReview PowerReview { get; set; }
        public bool IsInStore { get; set; }
        public List<string> VariantIdsSegments { get; set; }
        public List<VariationAttribute> AvailableVariations { get; set; }
        public List<KeyValuePair<string, string>> BreadCrumb { get; set; }
        public List<string> ProductFlags { get; set; }
        public List<ProductPromotion> Promotions { get; set; }
        public string VideoID { get; set; }
        public List<SiteError> _errors { get; set; }
        public static List<SiteError> _staticerrors { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }

        public Product()
        {
            AvailableVariations = new List<VariationAttribute>();
            Colors = new List<ColorOption>();
            Designs = new List<DesignOption>();
            PowerReview = new PowerReview.PowerReview();
            VariantIdsSegments = new List<string>();
            _errors = new List<SiteError>();
            _staticerrors = new List<SiteError>();

        }
        public Product(DWProduct product, string color, ICore _core, List<SiteError> errors) : this()
        {
            AdditionalInfo = product.CustomerService;
            CareInstructions = product.CareInstructions;
            Description = product.ShortDescription;
            Name = product.Name;
            Brand = product.Brand;
            Category = product.PrimaryCategoryId ?? "";
            ProductId = product.Id;
            ProductFlags = LookupCallouts(product.ProductFlags);
            Promotions = product.Promotions.Where(a => !string.IsNullOrEmpty(a.Callout)).ToList();
            IsInStore = product.IsInStore == null || product.IsInStore == true;
            

            if (!string.IsNullOrEmpty(product.PrimaryCategoryId))
                BreadCrumb = RecommendedProducts.Load(product.Id, _core).Breadcrumb;

            var price = new Price(product.Price.ToString(CultureInfo.InvariantCulture));
            if (product.Price == 0 && product.Variants != null)
                price = new Price(product.Variants[0].Price.ToString());

            if (product.MaxPrice > 0)
            {
                var maxPrice = new Price(product.MaxPrice.ToString());
                price.Label = maxPrice.Formatted;
            }
            Pricing.Add(price);
            Image = new Image{Title = product.Brand};
            if (!string.IsNullOrEmpty(product.DisplayBrand))
            {
                var cms = new PaylessMadCms(_core);
                var brandImages = cms.BrandImageLookup();
                if (brandImages.ContainsKey(product.DisplayBrand))
                {
                    var brandImage = brandImages[product.DisplayBrand];
                    Image.Src = string.Format(Config.Urls.BrandImageTemplate, brandImage);
                }
            }

            var variants = product.Variants;
            if (variants != null)
            {
                VariantIdsSegments = variants.Select((x, i) => new {x, i})
                    .GroupBy(x => x.i/50)
                    .Select(x => string.Join(",", x.Select(y => y.x.ProductId)))
                    .ToList();

                AvailableVariations = product.VariationAttributes;

                var isGiftCard = product.Brand != null && product.Brand == "GIFT CARD";
                if (AvailableVariations != null)
                {
                    var sizeVariations = AvailableVariations.Find(x => x.Id == "size" && x.Values.Any());
                    var isOneSizeOnly = (sizeVariations != null
                                         && sizeVariations.Values.Count.Equals(1)
                                         && sizeVariations.Values.First().Name == "One Size") ||
                                        (sizeVariations == null);


                    if (product.ImageGroups != null)
                    {
                        var imageGroups = product.ImageGroups;

                        var swatchImages = DwSwatchImages(imageGroups);
                        var colorVariations = DWColorVariationAttribute();
                        SetSelectedColorValue(color, colorVariations, swatchImages, imageGroups, variants, isOneSizeOnly,
                            isGiftCard);

                        var designVariations = DwDesignVariationAttribute();
                        if (designVariations != null)
                        {
                            DWGetDesignValues(designVariations, imageGroups, variants);

                            if (Designs.Any())
                            {
                                var firstOrDefault = Designs.FirstOrDefault();
                                if (firstOrDefault != null) firstOrDefault.IsSelected = true;
                            }
                        }
                    }
                }
            }

            DwSetProductRating(_core, product);
            VideoID = product.InvodoVideoExists ? ProductId : null;
        }

        private void DwSetProductRating(ICore _core, DWProduct product)
        {
            try
            {
                if (product.ReviewCount > 0)
                {

                    var avgrating = GetPwAvgRate(_core, product.Id);
                    if (avgrating.Any())
                    {
                        var firstOrDefault = avgrating.FirstOrDefault();
                        if (firstOrDefault != null)
                            product.AverageRating = decimal.Parse(firstOrDefault.average_rating);
                    }

                    ProductRating = new ProductRatingBase
                    {
                        NumberOfTimesRated = product.ReviewCount,
                        Rating = product.AverageRating
                    };
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Product.DWSetProductRating", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private void DWGetDesignValues(VariationAttribute designVariations, List<ImageGroup> imageGroups,
            List<Variant> variants)
        {
            try
            {
                Designs = designVariations.Values
                    .Select(x =>
                    {
                        List<Image> images;
                        var designImages = DwDesignImages(imageGroups, x);
                        if (designImages != null)
                        {
                            images = designImages.Select(z => new Image
                            {
                                Src = APIHelper.GetOptimizedImageSrc(z.Link),
                                Title = z.Title
                            })
                                .ToList();
                        }
                        else
                        {
                            images = new List<Image> {new Image {Src = Config.Urls.DefaultGiftCardImage}};
                        }

                        var values = variants.Where(z => z.VariationValues
                            .Any(a => a.Value.Equals(x.Value)))
                            .Select(z =>
                            {
                                int valueInt;
                                var valueStr = string.Empty;
                                if (z.VariationValues.ContainsKey("value"))
                                    valueStr = z.VariationValues["value"];
                                int.TryParse(valueStr, out valueInt);

                                var name = string.Format("${0}", z.Price);

                                return new ValueOption
                                {
                                    Name = name,
                                    Value = z.ProductId.ToString(),
                                    IsInStock = z.IsOrderable
                                };
                            })
                            .ToList();

                        if (!values.Any())
                            return new DesignOption
                            {
                                Lot = null,
                                Name = x.Name,
                                Value = x.Value,
                                Values = values,
                                Images = images
                            };
                        var sku = values.First().Value;
                        var skuLength = sku.Length;
                        var lot = skuLength > 7 ? sku.Substring(0, skuLength - 3) : sku.ToString();

                        return new DesignOption
                        {
                            Lot = lot,
                            Name = x.Name,
                            Value = x.Value,
                            Values = values,
                            Images = images
                        };
                    }).ToList();
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Product.DWGetDesignValues", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private static List<DWImage> DwDesignImages(List<ImageGroup> imageGroups, VariationAttributeValue x)
        {
            var designImages = new List<DWImage>();
            try
            {
                if (imageGroups != null)
                {
                    imageGroups.Where(z => z.VariationAttributes.Select(e => e.values.Where(v => v.value.Equals(x.Value)).FirstOrDefault()).Any() && z.ViewType == "hi-res" && z.Images.Any())
                        .Select(z => z.Images)
                        .FirstOrDefault();
                }


            }
            catch (Exception ex)
            {
                _staticerrors.Add(ex.Handle("Product.DWDesignImages", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return designImages;
            }

            return designImages;
        }

        private VariationAttribute DwDesignVariationAttribute()
        {
            var designVariations = new VariationAttribute();
            try
            {
                if (AvailableVariations != null)
                {
                    designVariations = AvailableVariations.Find(x => x.Id == "design" && x.Values.Any());
                }
                   
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Product.DWDesignVariationAttribute", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return designVariations;
            }
            return designVariations;
        }

        private void SetSelectedColorValue(string color, VariationAttribute colorVariations,
            List<ImageGroup> swatchImages, List<ImageGroup> imageGroups,
            List<Variant> variants, bool isOneSizeOnly, bool isGiftCard)
        {
            try
            {
                if (colorVariations == null) return;
                Colors = colorVariations.Values
                    .Select(colorVariation =>
                    {
                        var swatch = swatchImages.Where(z => z.VariationAttributes.Select(e => e.values.Where(v => v.value.Equals(colorVariation.Value)).Any()).FirstOrDefault())
                            .Select(z => z.Images.FirstOrDefault()).FirstOrDefault() ?? new DWImage();

                        var images = new List<ProductImage>();
                        var colorImages = ColorImages(imageGroups, colorVariation);
                        if (colorImages != null)
                        {
                            images = colorImages.Select(z => new ProductImage
                            {
                                Src = APIHelper.GetOptimizedImageSrc(z.Link, 200) ?? string.Empty,
                                SrcMed = APIHelper.GetOptimizedImageSrc(z.Link, 450) ?? string.Empty,
                                SrcLarge = APIHelper.GetOptimizedImageSrc(z.Link, 1400) ?? string.Empty,
                                Title = z.Title ?? string.Empty
                            }).ToList();
                        }

                        var sizes = DwSizeOptions(variants, colorVariation, isOneSizeOnly, isGiftCard);

                        string lot = null;
                        if (!sizes.Any())
                            return new ColorOption
                            {
                                Name = colorVariation.Name,
                                Value = colorVariation.Value,
                                Lot = lot,
                                Swatch = new Image {Src = swatch.Link, Title = swatch.Title},
                                Sizes = sizes,
                                Images = images
                            };

                        var sku = sizes.First().Value;
                        var skuLength = sku.Length;
                        lot = skuLength > 7 ? sku.Substring(0, skuLength - 3) : sku.ToString();

                        return new ColorOption
                        {
                            Name = colorVariation.Name,
                            Value = colorVariation.Value,
                            Lot = lot,
                            Swatch = new Image { Src = swatch.Link, Title = swatch.Title },
                            Sizes = sizes,
                            Images = images
                        };
                    }).ToList();

                SetSelectedColor(color);
            }
            catch (Exception ex)
            {

                _errors.Add(ex.Handle("Product.SetSelectedColorValue", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

        }

        private static List<SizeOption> DwSizeOptions(List<Variant> variants, VariationAttributeValue colorVariation,bool isOneSizeOnly, bool isGiftCard)
        {
            List<SizeOption> sizes;
            try
            {
                sizes = variants.Where(z => z.VariationValues
                    .Any(a => a.Value.Equals(colorVariation.Value)))
                    .Select(z =>
                    {
                        int sizeInt;
                        var sizeStr = string.Empty;
                        if (z.VariationValues.ContainsKey("size"))
                        {
                            sizeStr = z.VariationValues["size"];
                        }

                        int.TryParse(sizeStr, out sizeInt);

                        var width = string.Empty;
                        if (z.VariationValues.ContainsKey("width"))
                            width = z.VariationValues["width"];
                        

                        var name = string.Empty;
                        if (isOneSizeOnly)
                            name = "One Size - ";                       
                        else if (!isGiftCard)
                            name = sizeInt != 0 ? string.Format("{0:N1} {1} - ", sizeInt/10.0, width) : string.Format("{0} {1} - ", sizeStr, width);
                        

                        var shortName = name.Replace(" - ", string.Empty);
                        name = string.Format("{0}${1}{2}", name, z.Price,
                            z.IsOrderable ? string.Empty : " (Out of Stock)");

                        var sizeLabel = string.Format("{0:N1}", sizeInt/10.0);
                        return new SizeOption
                        {
                            Name = name,
                            ShortName = shortName,
                            Value = z.ProductId,
                            IsInStock = z.IsOrderable,
                            SizeLabel = sizeLabel,
                            WidthLabel = width,
                            Price = "$" + z.Price.ToString()
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _staticerrors.Add(ex.Handle("Product.DWSizeOptions", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return new List<SizeOption>();
            }

            return sizes;
        }

        private static List<DWImage> ColorImages(List<ImageGroup> imageGroups, VariationAttributeValue colorVariation)
        {
            List<DWImage> colorImages = new List<DWImage>();
            try
            {
                if (imageGroups != null)
                {
                    colorImages = imageGroups.Where(z => z.VariationAttributes.Select(e => e.values.Where(v => v.value.Equals(colorVariation.Value)).Any()).FirstOrDefault()
                                                         && z.ViewType == "hi-res" && z.Images.Any()).Select(z => z.Images).FirstOrDefault();
                }


            }
            catch (Exception ex)
            {

                _staticerrors.Add(ex.Handle("Product.ColorImages", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return new List<DWImage>();
            }

            return colorImages;
        }

        private VariationAttribute DWColorVariationAttribute()
        {
            var colorVariations = new VariationAttribute();
            try
            {
                if (AvailableVariations != null)
                    colorVariations = AvailableVariations.Find(x => x.Id == "color" && x.Values.Any());
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Product.ColorImages", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return new VariationAttribute();
            }

            return colorVariations;
        }

        private static List<ImageGroup> DwSwatchImages(List<ImageGroup> imageGroups)
        {
            var swatchImages = new List<ImageGroup>();
            try
            {
                if (imageGroups != null)
                {
                    swatchImages = imageGroups.FindAll(x => x.ViewType == "swatch" && x.Images.Any());
                    return swatchImages;
                }
            }
            catch (Exception ex)
            {
                _staticerrors.Add(ex.Handle("Product.DWSwatchImages", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return new List<ImageGroup>();
            }
            return swatchImages;
        }

        public static List<string> LookupCallouts(List<string> list)
        {
            var outList = new List<string>();
            try
            {
                if (!list.Any()) return outList;
                    foreach (var key in list)
                    {
                        try
                        {
                            outList.Add(Config.CallOutLookup[key]);
                        }
                        catch (Exception)
                        {
                            outList.Add(key);
                        }
                    }
                

            }
            catch (Exception ex)
            {
                _staticerrors.Add(ex.Handle("Product.LookupCallouts", ErrorSeverity.FollowUp, ErrorType.RequestError));
                return new List<string>();
            }
            return outList;
        }

        public void SetSelectedColor(string color)
        {
            try
            {
                if (!Colors.Any()) return;
                if (string.IsNullOrEmpty(color))
                {
                    var firstOrDefault = Colors.FirstOrDefault();
                    if (firstOrDefault != null) firstOrDefault.IsSelected = true;
                }
                else
                {
                    var colorToSelect = Colors.FirstOrDefault(c => c.Value == color);
                    if (colorToSelect != null)
                    {
                        colorToSelect.IsSelected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Product.SetSelectedColor", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private List<AvgRate> GetPwAvgRate(ICore _core, string productId)
        {
            var avgRatesRequest = new PowerReviewAvgRateRequest();
            var avgrates = new PowerReviewGetAvgRates(_core);
            avgRatesRequest.ProductId = productId;
            return ((Response<PowerReviewAvgRateResponse>)avgrates.Execute(avgRatesRequest)).resultset.AvgRates;
        }



    }
}
