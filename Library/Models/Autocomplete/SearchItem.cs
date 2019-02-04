using System.Collections.Generic;
namespace Library.Models.Autocomplete
{
    public class SearchItem
    {
        public string suggestion { get; set; }
        public int count { get; set; }
        public string hits { get; set; }
        public List<SearchItem> Items { get; set; }
        public SearchItem()
        {
            Items = new List<SearchItem>();
            
        }
    }
}


