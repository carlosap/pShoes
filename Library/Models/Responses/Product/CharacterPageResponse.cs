using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "CharacterPageResponse")]
    public class CharacterPageResponse : IResponse
    {
        public List<LandingPageItem> Items { get; set; }
        public string Template { get; set; }

        public CharacterPageResponse()
        {
            Items = new List<LandingPageItem>();
        }
    }
}
