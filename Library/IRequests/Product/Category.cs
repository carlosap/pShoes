using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Web;
using Enums;
using Library.Cache;
using Library.DemandWare.Models;
using Library.DemandWare.Models.DTOs;
using Library.DemandWare.RequestManager;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "Category", RequestType = typeof(CategoryRequest),
        ResponseType = typeof(Response<CategoryResponse>))]
    public class Category : IRequest
    {
        #region constructor and parameters

        private CategoryRequest _request;
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public Category(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }

        public Category(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }

        public Category()
        {
            _errors = new List<SiteError>();
        }

        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<CategoryResponse>();
            try
            {
                _request = (CategoryRequest) parameters;
                var hrefLookup = HrefLookup.Load(_core);
                var catid = hrefLookup.Forward.Get(ParsingHelper.GetHrefWithoutQueryString(_request.Href));
                var cacheKey = string.Format(Config.CacheKeys.Category, catid);

                result = CacheMemory.Get<Response<CategoryResponse>>(cacheKey);
                if (result == null || string.IsNullOrWhiteSpace(result.resultset.CategoryID))
                {
                    var forwardDate = GetDateFromRequest();
                    var maddash = new PaylessMadCms(_core);
                    result.resultset = maddash.GetCategoryData(catid, forwardDate);
                    var config = BuildAPIConfig(parameters, catid);
                    var apiResponse = DWClient.GetAPIResponse<ProductSearchResult>(config);
                    if (apiResponse.ResponseStatus.Code == HttpStatusCode.OK)
                    {
                        result.resultset.Filters = new ExtendedFilter(apiResponse.Model.Refinements, null, hrefLookup);
                    }

                    result.resultset.CategoryID = catid;
                    if (result.errors.Count == 0)
                    {
                        CacheMemory.SetAndExpiresHoursAsync(cacheKey, result, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Category.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }

        private DWClientConfig BuildAPIConfig(IRequestParameter parameters, string catid)
        {
            return new DWClientConfig(_core)
            {
                Path = string.Format(Config.DWPath.Search, _request.Href),
                Query = "format=json&refine_1=cgid=" + catid,
                Method = HttpMethod.GET
            };
        }

        private DateTime GetDateFromRequest()
        {
            var result = DateTime.Now;
            if (string.IsNullOrEmpty(_request.d) || !_request.d.Contains("/")) return result;
            result = DateTime.Parse(HttpUtility.UrlDecode(_request.d));
            return result;
        }
    }
}
