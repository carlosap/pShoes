using System.Collections.Generic;
using System.ComponentModel.Composition;
using Library.Cache;
using Library.Helpers;
using Library.Models.Requests;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CurrentHrefLookup",
        RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<HrefLookup>))]
    public class CurrentHrefLookup : IRequest
    {
        const string cacheKey = "HrefLookup";
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            var href = CacheMemory.Get<HrefLookup>(cacheKey);
            var result = new Response<HrefLookup> { resultset = href };
            return result;
        }
    }
}
