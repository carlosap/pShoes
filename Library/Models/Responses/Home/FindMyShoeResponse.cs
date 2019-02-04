using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using System.Collections.Generic;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "FindMyShoeResponse")]
    public class FindMyShoeResponse : IResponse
    {
        public List<ShoeBoxMenu> Items { get; set; }
    }

    public class ShoeBoxMenu
    {
        public string Href { get; set; }
        public string Name { get; set; }

    }
}
