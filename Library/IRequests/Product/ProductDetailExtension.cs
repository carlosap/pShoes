using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Text;
using Enums;
using Library.DemandWare.Models;
using Library.DemandWare.Models.DTOs;
using Library.DemandWare.RequestManager;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ProductDetailExtension",
        RequestType = typeof(ProductDetailExtensionRequest),
        ResponseType = typeof(Response<ProductDetailExtensionResponse>))]
    public class ProductDetailExtension : IRequest
    {
        #region constructor and parameters

        private ProductDetailExtensionRequest _request;
        private bool _hasMultipleProducts;

        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public ProductDetailExtension(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }

        public ProductDetailExtension(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }

        public ProductDetailExtension()
        {
            _errors = new List<SiteError>();
        }

        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var config = BuildAPIConfig(parameters);
            var result = GetResponse(config);
            return result;
        }
        private DWClientConfig BuildAPIConfig(IRequestParameter parameters)
        {
            _request = (ProductDetailExtensionRequest) parameters;
            _hasMultipleProducts = _request.VariantIds.Contains(",");
            var path = string.Format("{0}/{1}",
                Config.DWPath.ProductDetailExtension,
                _hasMultipleProducts ? string.Format("({0})", _request.VariantIds) : _request.VariantIds);

            var sb = new StringBuilder();
            sb.Append("expand=prices");
            //Per OCAPI 17.2 - Added all_images=true to Display Image_Groups
            sb.Append("&all_images=true");
            return new DWClientConfig(_core)
            {
                Path = path,
                Query = sb.ToString(),
                Method = HttpMethod.GET
            };
        }
        private IResponseBase GetResponse(DWClientConfig config)
        {
            var result = new Response<ProductDetailExtensionResponse>();
            try
            {
                APIResponse apiResponse = null;
                if (_hasMultipleProducts)
                    apiResponse = DWClient.GetAPIResponse<ProductResult>(config);
                else
                    apiResponse = DWClient.GetAPIResponse<DWProduct>(config);

                if (apiResponse.ResponseStatus.Code == HttpStatusCode.OK)
                {
                    if (_hasMultipleProducts)
                    {                        
                        result.resultset = new ProductDetailExtensionResponse(((APIResponse<ProductResult>)apiResponse).Model,apiResponse.Raw);
                    }
                    else
                    {
                        result.resultset = new ProductDetailExtensionResponse(ProductDetailExtensionResponse.GetProductResult(((APIResponse<DWProduct>)apiResponse).Model));
                    }                    
                    result.template = Config.ResponseTemplates.ProductDetailExtension;
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.GenericError, apiResponse.ResponseStatus.Message)
                    });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ProductDetailExtension.GetResponse", ErrorSeverity.FollowUp,
                    ErrorType.RequestError));
            }
            return result;
        }
    }
}
