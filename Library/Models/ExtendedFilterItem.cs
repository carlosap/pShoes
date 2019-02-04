using System;
using System.Linq;
using System.Web;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ExtendedFilterItem : FilterListItem
    {
        public ExtendedFilterItem()
        { 
        }

        public ExtendedFilterItem(ProductSearchRefinementValue value, ExtendedFilterGrouping selectedFilters)
        {
            Label = value.Label;
            Value = HttpUtility.UrlEncode(value.Value);
            Note = value.HitCount.ToString();
            IsSelected = selectedFilters != null
                && selectedFilters.FilterOptions != null
                    && selectedFilters.FilterOptions.Any(x => x.Value.Equals(value.Value.Replace(" ","+")));
        }

        public ExtendedFilterItem(string value)
        {
            Label = value ?? string.Empty;
            Value = HttpUtility.UrlEncode(value ?? string.Empty);
        }
    }
}
