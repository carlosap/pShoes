using System;
using System.Collections.Generic;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ExtendedSorter : Sorter
    {
        public ExtendedSorter() : base()
        {
            Options = new List<Option>();
        }

        public ExtendedSorter(List<ProductSearchSortingOption> options, string selectedOption) : this()
        {
            if (selectedOption == null)
            {
                selectedOption = "";
            }

            if (options.Any())
            {
                foreach (var option in options)
                {
                    Options.Add(new Option
                    {
                        Name = option.Label,
                        Value = option.Id,
                        IsSelected = option.Id.Equals(selectedOption)
                    });
                }

                var selected = Options.Where(x => x.IsSelected).FirstOrDefault();

                if (selected != null)
                {
                    SortBy = selected.Name;
                }
            }
        }
    }
}
