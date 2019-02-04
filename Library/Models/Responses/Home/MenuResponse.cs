using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "MenuResponse")]
    public class MenuResponse : IResponse
    {
        public List<MenuItem> Menu { get; set; }
        public List<MenuItem> AllBrands { get; set; }

        public MenuResponse()
        {
            Menu = new List<MenuItem>();
            AllBrands = new List<MenuItem>();
        }

        public MenuResponse(Category category, string path = "")
        {
            var root = new MenuItem(category, path);
            Menu = root.Subs;
        }
    }
}
