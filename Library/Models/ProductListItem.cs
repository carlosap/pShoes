using System;
using System.Linq;
using System.Collections.Generic;
using Library.DemandWare.Models.DTOs;
using Library.Helpers;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ProductListItem : ProductListItemBase
    {
        public bool IsBOGO { get; set; }
        public bool IsSale { get; set; }
        public bool IsClearance { get; set; }
        public decimal Rating { get; set; }
        public bool IsAvailableInMultipleColors { get; set; }

        private List<VariationAttribute> _availableVariations;
        public List<VariationAttribute> AvailableVariations 
        {
            get { return _availableVariations; }
            set
            {
                _availableVariations = value;

                if (!string.IsNullOrEmpty(DefaultColor))
                {
                    var colors = _availableVariations.Where(item => item.Id == "color").FirstOrDefault();
                    if (colors != null)
                    {
                        var selected = colors.Values.Where(color => color.Value.ToLowerInvariant() == DefaultColor.ToLowerInvariant()).FirstOrDefault();
                        if (selected != null)
                        {
                            if (selected.Image != null)
                            {
                                this.Image.Description = selected.Image.Alt;
                                this.Image.Src = selected.Image.Link;
                                this.Image.Title = selected.Image.Title;
                            }
                        }
                    }
                }
            }
        }
        
        public List<string> ProductFlags { get; set; }
        public List<string> CallOuts { get; set; }

        public string DefaultColor { get; set; }
        
        public ProductListItem()
        {
            Image = new Image();
            Pricing = new List<Price>();
            AvailableVariations = new List<VariationAttribute>();
            ProductFlags = new List<string>();
        }

        public ProductListItem(ProductSearchHit product, PLPExtraInfo info, List<VariationAttribute> variationInfo)  : this()
        {
            ProductId = product.ProductId;
            Name = product.ProductName;
            
            if (info != null)
            {
                DefaultColor = info.defaultColor;
            }

            if (info != null && info.prices != null)
            {
                if (info.prices.standard != info.prices.sale)
                {
                    //unfortunately, the sale and regular price have to be in this specific order in the list
                    //because that's how the template uses them
                    if (!string.IsNullOrEmpty(info.prices.sale))
                    {
                        var salesPrice = new Price()
                        {
                            Label = "sale",
                            Currency = "USD",
                            Value = Convert.ToDouble(info.prices.sale)
                        };
                        Pricing.Add(salesPrice);
                    }

                    if (!string.IsNullOrEmpty(info.prices.standard))
                    {
                        var regularPrice = new Price()
                        {
                            Label = "reg",
                            Currency = "USD",
                            Value = Convert.ToDouble(info.prices.standard)
                        };
                        Pricing.Add(regularPrice);
                    }
                }
                //if there is a range and the values are different
                else if (info.prices.rangelow != "0" && info.prices.rangehigh != "0" && info.prices.rangelow != info.prices.rangehigh)
                {
                    var high = new Price(info.prices.rangehigh);
                    Pricing.Add(new Price()
                    {
                        Currency = "USD",
                        Value = Convert.ToDouble(info.prices.rangelow),
                        Label = high.Formatted
                    });
                }
            }

            //if we cannot get the pricing from the info object, use the default API data
            if (Pricing.Count() == 0)
            {
                var price = new Price(product.Price.ToString());

                if (product.MaxPrice > 0)
                {
                    var maxPrice = new Price(product.MaxPrice.ToString());
                    price.Label = maxPrice.Formatted;
                }

                Pricing.Add(price);
            }

            if (!string.IsNullOrEmpty(product.Image.Link))
            {
                Image.Src = APIHelper.GetOptimizedImageSrc(product.Image.Link);
                Image.Title = product.Image.Title;
            }

            if (variationInfo.Any())
            {
                AvailableVariations = variationInfo;
            }


            if (info != null)
            {
                IsSale = info.IsSale;
                IsBOGO = info.IsBogo;
                IsClearance = info.IsClearance;
            }

        }
    }
}
