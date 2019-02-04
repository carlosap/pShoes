using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Library.Models.Requests;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.Cache;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ClearCache", RequestType = typeof(EmptyRequest),
        ResponseType = typeof(Response<EmptyRequest>))]
    public class ClearCache : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public IResponseBase Execute(IRequestParameter parameters)
        {
            if (EnvironmentHelper.IsProd())
            {
                //sets Icore middle ware to shutdown on response.
                //regardless of the return value.
                CacheMemory.ClearMenu();
                _core.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _core.Context.Response.Close();
                
            }
            else
            {
                CacheMemory.ClearCmsCache();
            }
            var result = new Response<EmptyRequest> { resultset = new EmptyRequest() };
            return result;
        }
    }
}
