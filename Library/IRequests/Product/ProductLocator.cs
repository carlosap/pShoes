using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Enums;
using Library.eCRM;
using Library.Helpers;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "ProductLocator",
        RequestType = typeof(ProductLocatorRequest), ResponseType = typeof(Response<ProductLocatorResponse>))]
    public class ProductLocator : IRequest
    {
        #region constructor and parameters

        private ProductLocatorRequest _request;

        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        public ProductLocator(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }

        public ProductLocator(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }

        public ProductLocator()
        {
            _errors = new List<SiteError>();
        }

        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            IResponseBase result = new Response<ProductLocatorResponse>();

            try
            {
                _request = (ProductLocatorRequest) parameters;
                var storeLocator = new StoreLocator(_core, _errors);
                var storeLocatorRequest = new StoreLocatorRequest
                {
                    Latitude = _request.Latitude,
                    Longitude = _request.Longitude,
                    Zip = _request.Zip,
                    Radius = _request.SearchRadius
                };
                var storeLocatorResponse = storeLocator.Execute(storeLocatorRequest);

                if (!_errors.Any())
                {
                    _request.Stores = ((Response<StoreLocatorResponse>) storeLocatorResponse).resultset.Locations;

                    var ecrmRequest = BuildECRMrequest(parameters);
                    result = GetResponse(ecrmRequest);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("ProductLocator.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

            return result;
        }

        private RetrieveInventoryRequest BuildECRMrequest(IRequestParameter parameters)
        {
            int lotNumber;
            var sizeCode = 0;
            var sku = _request.ProductSku;
            var skuLength = sku.Length;
            var storeList = new List<int>();

            _request.Stores.ForEach(store => storeList.Add(store.Id));

            if (skuLength > 7)
            {
                int.TryParse(sku.Substring(0, skuLength - 3), out lotNumber);
                int.TryParse(sku.Substring(skuLength - 3, 3), out sizeCode);
            }
            else
                int.TryParse(sku, out lotNumber);


            return new RetrieveInventoryRequest
            {
                lotQuantityList = new[]
                {
                    new LotQuantity
                    {
                        lotNumber = lotNumber,
                        desiredQuantity = 1,
                        sizeCode = sizeCode
                    }
                },
                storeList = storeList.ToArray()
            };
        }

        private IResponseBase GetResponse(RetrieveInventoryRequest ecrmRequest)
        {
            var result = new Response<ProductLocatorResponse>();
            var useProductionAPI = EnvironmentHelper.UseProductionAPI(_core.Context.Request);
            try
            {
                var ecrmResult = new RetrieveInventoryResult();          
                var ecrmClient = APIHelper.GetECRMclient(useProductionAPI);

                ecrmClient.RetrieveInventory(ref ecrmRequest, ref ecrmResult);

                if (ecrmResult.result)
                {
                    var storeStatusList = ecrmResult.storeStatusList.ToList();
                    foreach (var store in _request.Stores)
                    {
                        var match = storeStatusList.Find(x => x.storeNumber.Equals(store.Id));
                        if (match == null || !match.allItemsAvailable) continue;
                        result.resultset.Locations.Add(store);
                        if (!result.resultset.Locations.Count.Equals(_request.NumStoresToReturn)) continue;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (useProductionAPI)
                {
                    _errors.Add(ex.Handle("ProductLocator.GetResponse", ErrorSeverity.FollowUp, ErrorType.RequestError));
                }   
            }
            return result;
        }
    }
}
