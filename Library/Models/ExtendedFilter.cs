using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;
using Library.Helpers;

namespace Library.Models
{
    [Serializable]
    public class ExtendedFilter : Filter
    {
        public MenuItem SubMenu { get; set; }
        public List<KeyValuePair<string, string>> Path { get; set; }
        new public List<ExtendedFilterGrouping> FilterSections { get; set; }
        public List<ExtendedFilterGrouping> AppliedFilterSections { get; set; }

        public ExtendedFilter()
        {
            SubMenu = new MenuItem();
            FilterSections = new List<ExtendedFilterGrouping>();
            AppliedFilterSections = new List<ExtendedFilterGrouping>();
            Path = new List<KeyValuePair<string, string>>();
            Path.Add(new KeyValuePair<string, string>("home", "/"));
        }

        public ExtendedFilter(List<ProductSearchRefinement> refiners, Dictionary<string, string> selectedRefiners, HrefLookup hrefLookup) : this()
        {
            if (refiners != null)
            {
                // Find Category Refiner
                var selectedCategory = string.Empty;
                if (selectedRefiners != null)
                {
                    if (selectedRefiners.ContainsKey("cgid"))
                    {
                        selectedCategory = selectedRefiners["cgid"];
                    }
                }
                var categoryRefiner = refiners.Where(x => x.AttributeId.Equals("cgid")).FirstOrDefault() ?? new ProductSearchRefinement();
                
                // Create Path
                DrillCategoryDown(categoryRefiner.Values, Path, selectedCategory, hrefLookup);

                // Create Sub Menu Filter
                SubMenu = new MenuItem(categoryRefiner, Path);

                // Remove Non-Clickable Categories From Path
                Path.RemoveAll(x => x.Key == "featured" || x.Key == "styles");

                // Create Applied Filters
                if (selectedRefiners != null)
                {
                    foreach (var refiner in selectedRefiners)
                    {
                        AppliedFilterSections.Add(new ExtendedFilterGrouping(refiner));
                    }
                }

                // Create The Rest Of Filters
                foreach (var refiner in refiners)
                {
                    if (refiner.Values.Any() && !refiner.AttributeId.Equals("cgid"))
                    {
                        var selectedFilters = AppliedFilterSections.Find(x => refiner.AttributeId.EndsWith(x.Note));
                        FilterSections.Add(new ExtendedFilterGrouping(refiner, selectedFilters));

                        FilterSections.RemoveAll(section => section.FilterOptions.Count == 0);
                    }
                }
            }
        }

        private List<ProductSearchRefinementValue> DrillCategoryDown(List<ProductSearchRefinementValue> values, List<KeyValuePair<string, string>> path, string selectedCategory, HrefLookup hrefLookup)
        {
            var childWithValues = values.Where(x => x.Values.Any()).FirstOrDefault();

            if (childWithValues != null)
            {
                path.Add(new KeyValuePair<string, string>(childWithValues.Label.ToLowerInvariant(), hrefLookup.Reverse.Get(childWithValues.Value)));
                return DrillCategoryDown(childWithValues.Values, path, selectedCategory, hrefLookup);
            }
            else
            {
                var selectedMatch = values.Where(x => x.Value.Equals(selectedCategory)).FirstOrDefault();
                if (selectedMatch != null)
                {
                    path.Add(new KeyValuePair<string, string>(selectedMatch.Label.ToLowerInvariant(), hrefLookup.Reverse.Get(selectedMatch.Value)));
                }
            }

            return values;
        }

        private string FindHref(NameValueCollection hrefLookup, string categoryId)
        {
            var href = string.Empty;
            foreach (var key in hrefLookup.AllKeys)
            {
                if (hrefLookup.Get(key) == categoryId)
                {
                    href = key;
                    break;
                }
            }

            return href;
        }
    }
}
