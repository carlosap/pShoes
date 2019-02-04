using Enums;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutShipping", RequestType = typeof(CheckoutShippingRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutShipping : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private string CsrfToken;
        private PaylessSession _session;

        public CheckoutShipping()
        {
            _errors = new List<SiteError>();
        }
        public CheckoutShipping(ICore core)
            : this()
        {
            _core = core;
            _session = new PaylessSession(_core); 
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl((CheckoutShippingRequest)parameters);

            //4/23/2015, JAY: they added a new McAfee symbol which contained malformed html
            //CustomStreamReaderProcess removes the junk; commenting this out for now because they supposedly fixed it.
            //communicationRequest.OptionalStreamReaderProcess = ParsingHelper.CustomStreamReaderProcess;

            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(CheckoutShippingRequest request)
        {
            try
            {
                if (request.CheckoutResponse != null)
                {
                    var checkout = request.CheckoutResponse;

                    if (_session == null)
                    {
                        _session = new PaylessSession(_core);
                    }

                    if (!string.IsNullOrEmpty(checkout.LoyaltyEmail))
                    {
                        checkout.Email = checkout.LoyaltyEmail;
                    }

                    _session.SetCheckout(checkout);

                    var shippingInfo = checkout.ShippingInfo;
                    var selectedShippingOption = checkout.ShippingOptions.Options.Where(x => x.IsSelected).FirstOrDefault() ?? new ShippingOption();
                    var selectedSavedAddress = checkout.SavedAddresses.Where(x => x.IsSelected).FirstOrDefault() ?? new SavedAddressOption();
                    var isShipToStore = checkout.ShippingOptions.SelectedOption == Config.Constants.ShipToStore;
                    var shipToStoreId = checkout.ShippingOptions.ShipToStoreId;

                    var url = string.Format("{0}{1}{2}&csrf_token={3}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutShippingPost, checkout.Cart.DWQuery,checkout.CsrfToken);
                    //cperez:rollback due to gift-card issues.
                    //var url = string.Format("{0}{1}/{2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutShippingPost, checkout.Cart.DWQuery.Replace("?dwcont=",""));

                    var postSb = new StringBuilder();

                    postSb.AppendFormat("dwfrm_singleshipping_shippingAddress_shipToStore={0}", isShipToStore ? "true" : "false");
                    if (checkout.IsLoggedIn)
                        postSb.AppendFormat("&dwfrm_singleshipping_addressList={0}", HttpUtility.UrlEncode(selectedSavedAddress.Value));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_firstName={0}", HttpUtility.UrlEncode(shippingInfo.FirstName));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_lastName={0}", HttpUtility.UrlEncode(shippingInfo.LastName));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_address1={0}", HttpUtility.UrlEncode(shippingInfo.Address1));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_address2={0}", HttpUtility.UrlEncode(shippingInfo.Address2));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_country={0}", "US");
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_city={0}", HttpUtility.UrlEncode(shippingInfo.City));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_states_state={0}", shippingInfo.State);
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_zip={0}", shippingInfo.Zip);
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_addressFields_phone={0}", shippingInfo.Phone);
                    if (checkout.IsAddToAddressBook)
                        postSb.Append("&dwfrm_singleshipping_shippingAddress_addToAddressBook=true");
                    if (checkout.PaymentInfo.BillingSameAsShipping)
                        postSb.Append("&dwfrm_singleshipping_shippingAddress_useAsBillingAddress=true");
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_loyaltyEmailAddress={0}", HttpUtility.UrlEncode(checkout.LoyaltyEmail));
                    postSb.AppendFormat("&dwfrm_singleshipping_shippingAddress_shippingMethodID={0}", selectedShippingOption.Value);
                    postSb.AppendFormat("&dwfrm_singleshipping_securekey={0}", checkout.DWSecureKey);
                    postSb.AppendFormat("&{0}=Next%3A+Enter+Billing+Info", isShipToStore ? "dwfrm_singleshipping_shipToStore_shipToStoreSave" : "dwfrm_singleshipping_shippingAddress_save");
                    postSb.AppendFormat("&address={0}", checkout.ShippingOptions.ShipToStoreZip);
                    postSb.AppendFormat("&distance={0}", 100);
                    if (isShipToStore)
                        postSb.AppendFormat("&shipToStoreSelection={0}", shipToStoreId);
                    postSb.AppendFormat("&dwfrm_singleshipping_shipToStore_storeID={0}", string.IsNullOrEmpty(shipToStoreId) ? "null" : shipToStoreId);
                    postSb.Append("&dwfrm_singleshipping_shipToStore_searchAddress=");
                    postSb.AppendFormat("&EnvironmentID={0}", EnvironmentHelper.GetEnvironmentId(_core.Context.Request));

                    _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                    {
                        OverridePostData = postSb.ToString()
                    };
                }
                else
                {
                    var url = string.Format("{0}{1}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutShippingGet);
                    _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutShipping.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        public IResultResponse Communicate(ICommunicationRequest request)
        {
            try
            {
                request.OptionalRemoveScriptTags = false;
                var resultResponse = _core.RequestManager.Communicate(request);
                return resultResponse;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutShipping.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
