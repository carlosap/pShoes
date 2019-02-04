using System;
using System.Collections.Generic;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ExtendedFilterGrouping : FilterGrouping
    {
        new public List<ExtendedFilterItem> FilterOptions { get; set; }

        public ExtendedFilterGrouping()
        {
            FilterOptions = new List<ExtendedFilterItem>();
        }

        public ExtendedFilterGrouping(ProductSearchRefinement refiner, ExtendedFilterGrouping selectedFilters) : this()
        {
            Label = refiner.Label.ToLowerInvariant();
            Note = refiner.AttributeId;

            if (selectedFilters != null)
            {
                selectedFilters.Label = Label;
            }

            foreach (var value in refiner.Values)
            {
                if (value.HitCount > 0)
                    FilterOptions.Add(new ExtendedFilterItem(value, selectedFilters));
            }
        }

        public ExtendedFilterGrouping(KeyValuePair<string, string> refiner) : this()
        {
            Label = refiner.Key;
            Note = refiner.Key;

            foreach (var value in refiner.Value.Split('|'))
            {
                FilterOptions.Add(new ExtendedFilterItem(value));
            }

            if (Label == "price")
            {
                foreach (var option in FilterOptions)
                {
                    option.Label = option.Label
                                         .Replace("(", "$")
                                         .Replace("..", " - $")
                                         .Replace(")", string.Empty);
                }
            }
        }
    }
}
