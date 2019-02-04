using System;
using System.Collections.Generic;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class DesignOption : Option
    {
        public string Lot { get; set; }
        public List<Image> Images { get; set; }
        public List<ValueOption> Values { get; set; }
        
        public DesignOption()
        {
            Images = new List<Image>();
            Values = new List<ValueOption>();
        }
    }
}
