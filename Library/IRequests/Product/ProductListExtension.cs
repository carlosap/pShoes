using Enums;
using Library.DemandWare.Models;
using Library.DemandWare.Models.DTOs;
using Library.DemandWare.RequestManager;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Text;
using System.Linq;
using Library.Models;
using Library.Models.ProductVariant;
using Library.PowerReview;
using Newtonsoft.Json;
using Library.Extensions;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ProductListExtension", RequestType = typeof(ProductListExtensionRequest), ResponseType = typeof(Response<ProductListExtensionResponse>))]
    public class ProductListExtension : IRequest
    {
        #region constructor and parameters
        public Dictionary<string, ProductListItemExtension> ProductIdToExtension { get; set; }
        public ProductVariants ProductVariants { get; set; }
        private ProductListExtensionRequest _request;
        private bool _hasMultipleProducts;

        public ICore _core { get; set; }
        public bool _usePredefinedVariations { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public ProductListExtension(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
            ProductIdToExtension = new Dictionary<string, ProductListItemExtension>();
        }
        public ProductListExtension(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
            ProductIdToExtension = new Dictionary<string, ProductListItemExtension>();
        }
        public ProductListExtension()
        {
            _errors = new List<SiteError>();
            ProductIdToExtension = new Dictionary<string, ProductListItemExtension>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameter)
        {
            var config = BuildAPIConfig(parameter);
            var result = GetResponse(config);
            return result;
        }

        private DWClientConfig BuildAPIConfig(IRequestParameter parameters)
        {
            _request = (ProductListExtensionRequest)parameters;
            _hasMultipleProducts = _request.ProductsIds.Contains(",");            

            var path = string.Format("{0}/{1}", 
                Config.DWPath.ProductListExtension,
                _hasMultipleProducts ? string.Format("({0})", _request.ProductsIds) : _request.ProductsIds);

            var sb = new StringBuilder();
            sb.Append("expand=images,variations,promotions");
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
            var result = new Response<ProductListExtensionResponse>();
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
                        var productResult = ((APIResponse<ProductResult>)apiResponse).Model;
                        productResult.Products = SetProductBrands(productResult.Products, apiResponse.Raw);
                        result.resultset = new ProductListExtensionResponse(productResult, _request.Colors,apiResponse.Raw);
                    }
                    else
                    {
                        var product = ((APIResponse<DWProduct>)apiResponse).Model;                      
                        if (_usePredefinedVariations)
                        {
                            ReplaceColors(product);
                        }                          

                        result.resultset = new ProductListExtensionResponse(ProductListExtensionResponse.GetProductResult(product), _request.Colors,apiResponse.Raw);
                    }

                    result.template = Config.ResponseTemplates.ProductListExtension;
                    foreach (var productItem in result.resultset.ProductIdToExtension)
                    {
                        try
                        {
                            var productsnapshot = PowerReviewHelper.GetSnapshot(productItem.Key);
                            productItem.Value.Rating = (productsnapshot.average_rating == null)? 0: decimal.Parse((productsnapshot.average_rating));
                        }
                        catch (Exception ex)
                        {
                            productItem.Value.Rating = 0;
                        }
                    }
                }
                else
                {
                    _errors.Add(new SiteError { Message = new ErrorMessage(Config.Constants.GenericError, apiResponse.ResponseStatus.Message.ToString()) });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ProductListExtension.GetResponse", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

            return result;
        }

        private List<DWProduct> SetProductBrands(List<DWProduct> products, string rawResponse)
        {
            if (!string.IsNullOrEmpty(rawResponse))
            {
                ProductVariants = JsonConvert.DeserializeObject<ProductVariants>(rawResponse);
            }
            foreach (var product in products)
            {
                var productId = product.Id;
                var isBranded = ProductVariants.data.FirstOrDefault(items => items.id.Equals(productId));
                if (isBranded != null)
                {
                    if (string.IsNullOrEmpty(product.Brand))
                    {
                        product.Brand = isBranded.c_displayBrand.AddSpaceAfterUpperCase(true);
                    }
                        
                }
            }
            return products;
        }

        private DWProduct ReplaceColors(DWProduct product)
        {
            try
            {
                var colorVariations = _request.Products.Where(p => p.ProductId == product.Id).Select(v => v.AvailableVariations.FirstOrDefault());
                product.VariationAttributes.RemoveAll(p => p.Id == "color");
                product.VariationAttributes.AddRange(colorVariations.ToList());
                product.VariationAttributes.Reverse();

            }
            catch (Exception){}
            return product;
        }
    }
}
