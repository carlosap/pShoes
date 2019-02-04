using System;
using System.Collections.Generic;
using System.Linq;
using Library.Models.Responses;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Extensions;
using Enums;
using Library.Models;
using MadServ.Core.Helpers;
using System.Xml.Linq;

namespace Library.Services
{
    public class AccountService : ParsingService
    {
        public AccountService(ICore core) : base(core) { }

        public IResponseBase ParseLogin(IResultResponse response, IRequestParameter parameters)
        {
            Response<LoginFormResponse> result = new Response<LoginFormResponse>();

            try
            {
                var form = _xDoc.Descendants(_ns + "form")
                                .WhereAttributeEquals("id", "dwfrm_login")
                                .FirstOrNewXElement();

                var action = form.AttributeValue("action");

                var dwSecureKey = form.Descendants(_ns + "input")
                                      .WhereAttributeEquals("name", "dwfrm_login_securekey")
                                      .FirstOrNewXElement()
                                      .AttributeValue("value");

                var dwLoginParam = form.Descendants(_ns + "input")
                                       .WhereAttributeContains("id", "dwfrm_login_username_")
                                       .FirstOrNewXElement()
                                       .AttributeValue("id");

                if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(dwSecureKey))
                {
                    result.resultset.Action = action;
                    result.resultset.DWSecureKey = dwSecureKey;
                    result.resultset.DWLoginParam = dwLoginParam;
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.GenericError, Config.Constants.GenericError),
                        Severity = ErrorSeverity.FollowUp,
                        Type = ErrorType.Unclassifiable
                    });
                }
            }
            catch (Exception ex)
            {

                _errors.Add(ex.Handle(
                   "MadServ.AccountService.ParseAccountOrderDetail",
                   ErrorSeverity.FollowUp,
                   ErrorType.Parsing
                   ));
            }

            return result;
        }

        public IResponseBase ParseAccountOrderDetail(IResultResponse response, IRequestParameter parameters)
        {
            Response<AccountOrderDetailResponse> result = new Response<AccountOrderDetailResponse>();

            try
            {
                var notFoundMessage = GetDivByClass("not-found").ElementValue();

                if (string.IsNullOrEmpty(notFoundMessage))
                {
                    var mainContainer = GetDivByID("primary");

                    var orderInfo = GetTDByClass("order-information", mainContainer);

                    result.resultset.OrderNumber = GetDivsValueSpan("order-number ", orderInfo); //the space is important
                    result.resultset.OrderedOn = GetDivsValueSpan("order-date ", orderInfo); //the space is important
                    result.resultset.Status = GetDivsValueSpan("order-status", orderInfo);

                    //the rest of this method needs to be cleaned up when there's more time
                    
                    var billingInfoContainer = GetDivByClass("order-billing", mainContainer);

                    var billingName = GetDivByClass("mini-address-name", billingInfoContainer).ElementValue();

                    var billingInfo = new List<string>();

                    billingInfo.Add(billingName);

                    billingInfoContainer.Descendants(_ns + "address")
                                        .FirstOrNewXElement()
                                        .Elements()
                                        .ToList()
                                        .ForEach(x => billingInfo.Add(x.ElementValue()));

                    // Parse Payment Method
                    var paymentMethodContainer = mainContainer.Descendants(_ns + "td")
                                                              .WhereAttributeEquals("class", "order-payment-instruments")
                                                              .FirstOrNewXElement();

                    var paymentMethod = new List<string>();
                    paymentMethodContainer.Elements()
                                          .ToList()
                                          .ForEach(x => paymentMethod.Add(x.ElementValue()));

                    // Parse Summary
                    var summaryContainer = mainContainer.Descendants(_ns + "table")
                                                        .WhereAttributeEquals("class", "order-totals-table")
                                                        .FirstOrNewXElement();

                    var total = new Price(summaryContainer.Descendants(_ns + "tr")
                                                          .WhereAttributeEquals("class", "order-total")
                                                          .FirstOrNewXElement()
                                                          .ElementValue());

                    var costs = summaryContainer.Descendants(_ns + "tr")
                                                .Where(x => x.Elements(_ns + "td")
                                                             .Any())
                                                .Where(x => x.AttributeValue("class") != "order-total")
                                                .Select(x =>
                                                    {
                                                        var label = x.Elements(_ns + "td")
                                                                     .FirstOrNewXElement()
                                                                     .ElementValue();

                                                        var amount = x.ElementValue();

                                                        return new Price(amount)
                                                        {
                                                            Label = label
                                                        };
                                                    })
                                                .ToList();

                    // Parse Shipping Info
                    var itemsShipmentContainer = mainContainer.Descendants(_ns + "table")
                                                              .WhereAttributeContains("class", "order-shipment-table")
                                                              .FirstOrNewXElement();

                    var shippingInfo = new AddressBase();

                    shippingInfo.FirstName = itemsShipmentContainer.Descendants(_ns + "span")
                                                                   .WhereAttributeEquals("class", "firstname")
                                                                   .FirstOrNewXElement()
                                                                   .ElementValue();

                    shippingInfo.LastName = itemsShipmentContainer.Descendants(_ns + "span")
                                                                  .WhereAttributeEquals("class", "lastname")
                                                                  .FirstOrNewXElement()
                                                                  .ElementValue();

                    shippingInfo.Address1 = itemsShipmentContainer.Descendants(_ns + "div")
                                                                  .WhereAttributeEquals("class", "line1")
                                                                  .FirstOrNewXElement()
                                                                  .ElementValue();

                    shippingInfo.Address2 = itemsShipmentContainer.Descendants(_ns + "div")
                                                                  .WhereAttributeEquals("class", "line2")
                                                                  .FirstOrNewXElement()
                                                                  .ElementValue();

                    shippingInfo.City = itemsShipmentContainer.Descendants(_ns + "span")
                                                              .WhereAttributeEquals("class", "city")
                                                              .FirstOrNewXElement()
                                                              .ElementValue();

                    shippingInfo.State = itemsShipmentContainer.Descendants(_ns + "span")
                                                               .WhereAttributeEquals("class", "state")
                                                               .FirstOrNewXElement()
                                                               .ElementValue();

                    shippingInfo.Zip = itemsShipmentContainer.Descendants(_ns + "span")
                                                             .WhereAttributeEquals("class", "zip")
                                                             .FirstOrNewXElement()
                                                             .ElementValue();

                    shippingInfo.Country = itemsShipmentContainer.Descendants(_ns + "div")
                                                                 .WhereAttributeEquals("class", "country")
                                                                 .FirstOrNewXElement()
                                                                 .ElementValue();

                    shippingInfo.Phone = itemsShipmentContainer.Descendants(_ns + "div")
                                                               .WhereAttributeEquals("class", "phone")
                                                               .FirstOrNewXElement()
                                                               .ElementValue();

                    var shippingMethod = itemsShipmentContainer.Descendants(_ns + "div")
                                                               .WhereAttributeEquals("class", "shipping-method")
                                                               .FirstOrNewXElement()
                                                               .Elements(_ns + "span")
                                                               .WhereAttributeEquals("class", "value")
                                                               .FirstOrNewXElement()
                                                               .ElementValue();

                    var shippingStatus = itemsShipmentContainer.Descendants(_ns + "div")
                                                               .WhereAttributeEquals("class", "shipping-status")
                                                               .FirstOrNewXElement()
                                                               .Elements(_ns + "span")
                                                               .WhereAttributeEquals("class", "value")
                                                               .FirstOrNewXElement()
                                                               .ElementValue();

                    // Parse Items
                    var items = itemsShipmentContainer.Descendants(_ns + "tr")
                                                      .Where(x => x.Elements(_ns + "td")
                                                                   .Any())
                                                      .Select(x =>
                                                          {
                                                              var detailsContainer = x.Descendants(_ns + "div")
                                                                                      .WhereAttributeEquals("class", "product-list-item")
                                                                                      .FirstOrNewXElement();

                                                              var name = detailsContainer.Descendants(_ns + "a")
                                                                                         .FirstOrNewXElement()
                                                                                         .ElementValue();

                                                              var href = detailsContainer.Descendants(_ns + "a")
                                                                                         .FirstOrNewXElement()
                                                                                         .AttributeValue("href")
                                                                                         .Replace(Config.Urls.BaseUrl, string.Empty);

                                                              var productId = detailsContainer.Descendants(_ns + "div")
                                                                                              .WhereAttributeEquals("class", "sku")
                                                                                              .FirstOrNewXElement()
                                                                                              .Elements(_ns + "span")
                                                                                              .WhereAttributeEquals("class", "value")
                                                                                              .FirstOrNewXElement()
                                                                                              .ElementValue();

                                                              var color = detailsContainer.Descendants(_ns + "span")
                                                                                          .WhereAttributeEquals("class", "value Color")
                                                                                          .FirstOrNewXElement()
                                                                                          .ElementValue();

                                                              var size = detailsContainer.Descendants(_ns + "span")
                                                                                         .WhereAttributeEquals("class", "value Size")
                                                                                         .FirstOrNewXElement()
                                                                                         .ElementValue();

                                                              var width = detailsContainer.Descendants(_ns + "span")
                                                                                          .WhereAttributeEquals("class", "value Width")
                                                                                          .FirstOrNewXElement()
                                                                                          .ElementValue();

                                                              var design = detailsContainer.Descendants(_ns + "span")
                                                                                           .WhereAttributeEquals("class", "value Gift Card Design")
                                                                                           .FirstOrNewXElement()
                                                                                           .ElementValue();

                                                              var gcValue = detailsContainer.Descendants(_ns + "span")
                                                                                            .WhereAttributeEquals("class", "value Gift Card Value")
                                                                                            .FirstOrNewXElement()
                                                                                            .ElementValue();

                                                              var qtyStr = x.Elements(_ns + "td")
                                                                            .Skip(1)
                                                                            .FirstOrNewXElement()
                                                                            .ElementValue();

                                                              var qty = 0;
                                                              Int32.TryParse(qtyStr, out qty);

                                                              var price = new Price(x.Descendants("td")
                                                                                     .Skip(2)
                                                                                     .FirstOrNewXElement()
                                                                                     .ElementValue());

                                                              return new CartItem
                                                              {
                                                                  Name = name,
                                                                  Href = href,
                                                                  ProductId = productId,
                                                                  Color = color,
                                                                  Size = size,
                                                                  Width = width,
                                                                  Design = design,
                                                                  GCValue = gcValue,
                                                                  Quantity = qty,
                                                                  TotalPrice = price
                                                              };
                                                          })
                                                      .ToList();

                    result.resultset.BillingInfo = billingInfo;
                    result.resultset.PaymentMethod = paymentMethod;
                    result.resultset.Summary = new CartSummaryBase { Total = total, Costs = costs };
                    result.resultset.ShippingInfo = shippingInfo;
                    result.resultset.ShippingMethod = shippingMethod;
                    result.resultset.ShippingStatus = shippingStatus;
                    result.resultset.Items = items;

                    result.resultset.TrackingId = GetDivsValueSpan("track-ingnumber");
                }
                else
                {
                    _errors.Add(new SiteError 
                    {
                        Message = new ErrorMessage(notFoundMessage, notFoundMessage),
                        Severity = ErrorSeverity.UserActionRequired,
                        Type = ErrorType.UserActionRequired
                    });
                }
            }
            catch (Exception ex)
            {

                _errors.Add(ex.Handle(
                   "MadServ.AccountService.ParseAccountOrderDetail",
                   ErrorSeverity.FollowUp,
                   ErrorType.Parsing
                   ));
            }

            return result;
        }

        public IResponseBase ParseAccountOrderHistory(IResultResponse response, IRequestParameter parameters)
        {
            Response<AccountOrderHistoryResponse> result = new Response<AccountOrderHistoryResponse>();

            try
            {
                var mainContainer = _xDoc.Descendants(_ns + "div")
                                         .WhereAttributeEquals("id", "primary")
                                         .FirstOrNewXElement();

                // Parse History Items
                var items = mainContainer.Descendants(_ns + "ul")
                                         .WhereAttributeEquals("class", "search-result-items")
                                         .FirstOrNewXElement()
                                         .Elements(_ns + "li")
                                         .Select(x =>
                                             {
                                                 var date = GetDivsValueSpan("order-date", x);

                                                 var status = GetDivsValueSpan("order-status", x);

                                                 ///what the fuck is this?
                                                 return new AccountOrderHistoryItem
                                                 {
                                                 };
                                             })
                                         .ToList();
                                        
                    
                // Populate Result
            }
            catch (Exception ex)
            {

                _errors.Add(ex.Handle(
                   "MadServ.AccountService.ParseAccountOrderHistory",
                   ErrorSeverity.FollowUp,
                   ErrorType.Parsing
                   ));
            }

            return result;
        }

        private string GetDivsValueSpan(string className, XElement xElement = null)
        {
            return GetDivByClass(className, xElement)
                     .Elements(_ns + "span")
                     .WhereAttributeContains("class", "value")
                     .FirstOrNewXElement()
                     .ElementValue();
        }
    }
}
