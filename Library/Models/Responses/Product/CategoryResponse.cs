using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "CategoryResponse")]
    public class CategoryResponse
    {
        public string CategoryID { get; set; }

        public List<Banner> TabletBanners { get; set; }
        public List<ContentRow> ContentRows { get; set; }
        public ExtendedFilter Filters { get; set; }
        public List<Banner> HeroBanners { get; set; }
    }
}
