using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "SearchRequest")]
    public class SearchRequest : IRequestParameter
    {
        public string Href { get; set; }

        //public Filter Filter { get; set; }
        //public string Term { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        //public string SortBy { get; set; }

        public SearchRequest()
        {
          //  Filter = new Filter();
            PageSize = 12;
            Page = 1;
          //  SortBy = "best-matches";
        }
    }
}
