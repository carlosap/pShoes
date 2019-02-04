using System;
using System.Linq;
using System.Collections.Generic;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ShippingOptions : ShippingOptionsBase
    {
        public string SelectedMethod { get; set; }
        public string ShipToStoreZip { get; set; }
        public string ShipToStoreId { get; set; }
        public string ShipToStoreDescription { get; set; }
        public string ShipToStoreLabel { get; set; }
        public string ShippingSurchargeMessage { get; set; }

        public Price ShipToStorePrice { get; set; }

        public ShippingOptions()
        {
            Options = new List<ShippingOption>();
        }

        private List<ShippingOption> _options;
        public new List<ShippingOption> Options 
        {
            get { return _options; }
            set
            {
                _options = value;

                var selected = _options.Where(opt => opt.IsSelected).FirstOrDefault();
                if (selected != null)
                {
                    _selectedOption = selected.Value;
                }
            }
        }

        private string _selectedOption;
        public new string SelectedOption 
        {
            get
            {
                return _selectedOption;
            }

            set
            {
                _selectedOption = value;

                var option = Options.Where(opt => opt.Value == value).FirstOrDefault();

                if (option != null)
                {
                    Options.ForEach(opt => opt.IsSelected = false);
                    option.IsSelected = true;
                }
            }
        }

    }
}
