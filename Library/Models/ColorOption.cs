using System;
using System.Collections.Generic;
using MadServ.Core.Models;
using System.Linq;

namespace Library.Models
{
    [Serializable]
    public class ColorOption : Option
    {
        public string Lot { get; set; }
        public Image Swatch { get; set; }
        public List<ProductImage> Images { get; set; }

        private List<SizeOption> _sizes;
        public List<SizeOption> Sizes 
        {
            get { return _sizes; }
            set
            {
                _sizes = value;

                _tabletSizes.Clear();

                var labels = _sizes.GroupBy(s => s.SizeLabel).Select(s => s.First()).ToList().Select(s => s.SizeLabel).ToList();

                if (labels.Count == 1 && _sizes.Count() > 1)
                {
                    foreach (var size in _sizes)
                    {
                        var newSize = new TabletSizeOption() 
                        { 
                            SizeLabel = size.ShortName,
                            Value = size.Value,
                            Price = size.Price
                        };

                        newSize.Widths.Add(new SizeOption() {
                            Name = "Regular",
                            Value = size.Value,
                            Price = size.Price
                        });

                        _tabletSizes.Add(newSize);
                    }
                }
                else
                {
                    foreach (var label in labels)
                    {
                        var size = new TabletSizeOption();

                        var widths = _sizes.Where(s => s.SizeLabel == label);

                        size.SizeLabel = label;

                        foreach (var width in widths)
                        {
                            if (width.IsInStock) { size.IsInStock = true; }//only set if true

                            size.Value = width.Value;
                            size.SizeSegment = width.SizeSegment;

                            size.Widths.Add(new SizeOption() { Name = width.WidthLabel, Value = width.Value, Price = width.Price, IsInStock = width.IsInStock });
                        }

                        _tabletSizes.Add(size);
                    }
                }
            }
        }

        private List<TabletSizeOption> _tabletSizes;
        public List<TabletSizeOption> TabletSizes 
        {
            get
            {
                return _tabletSizes;
            }
        }

        public List<Option> SizeSegments { get; set; }

        public ColorOption()
        {
            Swatch = new Image();
            Images = new List<ProductImage>();
            
            _sizes = new List<SizeOption>();
            _tabletSizes = new List<TabletSizeOption>();

            SizeSegments = new List<Option>();
        }
    }
}
