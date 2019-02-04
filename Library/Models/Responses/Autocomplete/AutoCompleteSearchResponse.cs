using System;
using System.ComponentModel.Composition;
using Library.Models.Autocomplete;
using MadServ.Core.Interfaces;
namespace Library.Models.Responses.Autocomplete
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "AutoCompleteSearchResponse")]
    public class AutoCompleteSearchResponse : IResponse
    {
        public SearchItem SearchItem { get; set; }
        public AutoCompleteSearchResponse()
        {
           SearchItem = new SearchItem();
        }

    }
}
