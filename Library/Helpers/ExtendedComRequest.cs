using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using Core.Helpers;
using Enums;
using Library.Helpers;
using Library.Services;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
namespace Library.RequestHandler
{
    public class ExtendedComRequest : CommunicationRequest
    {
        private XNamespace _ns;
        private PaylessSession _session;
        #region Constructors And Creators
        public ExtendedComRequest(ICore core, List<SiteError> errors): base(core, errors)
        {
            _session = new PaylessSession(core);
            //var checkout = (new PaylessSession(core)).GetCheckout();
            //cartId = checkout.Cart.DWQuery;
        }

        public ExtendedComRequest(HttpRequestMethod requestType, string url, ICore core, List<SiteError> errors): base(requestType, url, core, errors)
        {
            _session = new PaylessSession(core);
        }
        #endregion

        public override void RebuildRequest()
        {
            

            OverrideUserAgent = Config.Params.UserAgent;
            OverrideUseSgmlReaderForConversion = false;
            OverrideUseCorsisForConversion = true;
            OverrideUseRemoteCorsisForConversion = true;
            OverrideUseCustomAttributesInCorsisConversion = true;
            OverrideExpect100 = true;
            OverrideSecurityProtocol = SecurityProtocolType.Tls11;
            OptionalPreserveOriginalRawData = true;

            Headers.Add("Authorization", "Basic c3RvcmVmcm9udDpyZXZvbHV0aW9u");
            Headers.Add(Config.Params.ClientIPHeader, RequestHeaderHelper.GetClientIP(Core));
            base.RebuildRequest();
        }

        public override Template GetTemplate(IResultResponse response)
        {
            var template = new Template
            {
                TemplateName = Config.TemplateEnum.GeneralError
            };

            try
            {
                if (this.OverrideBlockXDocumentConversion) // JSON
                {
                    if (this.OverrideStopAutoRedirects)
                    {
                        if (response.ResponseHeaders != null)
                        {
                            var location = string.Empty;
                            if (response.ResponseHeaders.TryGetValue("Location", out location) && location.Contains(".paypal."))
                            {
                                var service = new CheckoutService(Core);
                                template = new Template
                                {
                                    TemplateName = Config.TemplateEnum.PayPalRedirect,
                                    Service = service,
                                    Method = service.ParsePayPalRedirect
                                };
                            }
                            if (string.IsNullOrWhiteSpace(location))
                            {
                                if (response.RawData.Contains(".paypal."))
                                {
                                    var startIndex = response.RawData.IndexOf("url=");
                                    var endIndex = response.RawData.IndexOf("><meta http-equiv=\"Robots\"");
                                    location = response.RawData.Substring(startIndex, endIndex - startIndex).Replace("url=", "").Replace("\"", "").Trim();
                                    var service = new CheckoutService(Core);
                                    template = new Template
                                    {
                                        TemplateName = Config.TemplateEnum.PayPalRedirect,
                                        Service = service,
                                        Method = service.ParsePayPalRedirect
                                    };

                                }
                            }
                        }
                    }
                }
                else if (response.XDocument != null) // HTML and XML
                {
                    var xDoc = response.XDocument;
                    _ns = xDoc.Root.GetDefaultNamespace();

                    // Regular HTML Pages
                    var title = xDoc.Descendants(_ns + "title")
                                    .FirstOrNewXElement()
                                    .ElementValue();

                    if (title.IndexOf("****", StringComparison.InvariantCultureIgnoreCase) > -1
                     || string.IsNullOrEmpty(title))
                    {
                        if (xDoc.Descendants(_ns + "span")
                                .FirstOrNewXElement()
                                .AttributeValue("class")
                                .Equals("shopping-bag"))
                        {
                            var service = new CartService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CartMini,
                                Service = service,
                                Method = service.ParseCartMini
                            };
                        }
                        else if (xDoc.Descendants(_ns + "legend")
                                     .FirstOrNewXElement()
                                     .ElementValue()
                                     .IndexOf("Select Shipping Method", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var service = new CheckoutService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutShipping,
                                Service = service,
                                Method = service.ParseShippingOptions
                            };
                        }
                        else if (xDoc.ToString().IndexOf("shoebox", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var service = new HomeService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.FindMyPerfectShoe,
                                Service = service,
                                Method = service.ParseFindMyPerfectShoe
                            };
                        }
                    }
                    else if (title.IndexOf("My Payless Bag", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        var service = new CartService(Core);
                        template = new Template
                        {
                            TemplateName = Config.TemplateEnum.CartDetail,
                            Service = service,
                            Method = service.ParseCart
                        };
                    }
                    else if (title.IndexOf("Account Login", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        var header = ParsingHelper.GetTemplateHeader(xDoc, _ns);

                        if (header.IndexOf("Account Login", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var service = new AccountService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.Login,
                                Service = service,
                                Method = service.ParseLogin
                            };
                        }
                        else
                        {
                            var service = new CheckoutService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutBegin,
                                Service = service,
                                Method = service.ParseCheckoutBegin
                            };
                        }
                    }
                    else if (title.IndexOf("My Order History", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        var service = new AccountService(Core);
                        template = new Template
                        {
                            TemplateName = Config.TemplateEnum.OrderHistory,
                            Service = service,
                            Method = service.ParseAccountOrderHistory
                        };
                    }
                    else if (title.IndexOf("Sites-payless-Site", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        var service = new AccountService(Core);
                        template = new Template
                        {
                            TemplateName = Config.TemplateEnum.OrderDetail,
                            Service = service,
                            Method = service.ParseAccountOrderDetail
                        };
                    }
                    else if (title.IndexOf("My Account", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        template = new Template
                        {
                            TemplateName = Config.TemplateEnum.AccountDashboard
                        };
                    }
                    else if (title.IndexOf("Checkout", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        var checkoutService = new CheckoutService(Core);
                        var step = xDoc.Descendants(_ns + "div")
                                       .WhereAttributeContains("class", "step-")
                                       .WhereAttributeContains("class", " active")
                                       .FirstOrNewXElement()
                                       .ElementValue();

                        var breadCrumb = xDoc.Descendants(_ns + "div")
                                             .WhereAttributeEquals("class", "breadcrumb")
                                             .FirstOrNewXElement()
                                             .ElementValue();


                        var csrfToken = ParsingHelper.GetCheckout_CsrfToken(xDoc);
                        if (!string.IsNullOrEmpty(csrfToken))
                        {
                            var strCsrfToken = csrfToken.Split('=').GetValue(1).ToString();
                            if (_session == null)
                            {
                                _session = new PaylessSession(Core);
                            }
                            var checkout = _session.GetCheckout();
                            checkout.CsrfToken = strCsrfToken;
                            _session.SetCheckout(checkout);
                        }

                        if (step.IndexOf("Shipping", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutShipping,
                                Service = checkoutService,
                                Method = checkoutService.ParseShipping
                            };
                        }
                        else if (step.IndexOf("Billing", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutBilling,
                                Service = checkoutService,
                                Method = checkoutService.ParseBilling
                            };
                        }
                        else if (step.IndexOf("Review Order", StringComparison.InvariantCultureIgnoreCase) > -1
                                    || (string.IsNullOrEmpty(step) && title.IndexOf("Confirmation", StringComparison.InvariantCultureIgnoreCase) == -1))
                        {
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutReview,
                                Service = checkoutService,
                                Method = checkoutService.ParseReview
                            };
                        }
                        else if (title.IndexOf("Confirmation", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutConfirmation,
                                Service = checkoutService,
                                Method = checkoutService.ParseConfirmation
                            };
                        }
                        else if (breadCrumb.IndexOf("Login", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutBegin,
                                Service = checkoutService,
                                Method = checkoutService.ParseCheckoutBegin
                            };
                        }
                        else if (breadCrumb.IndexOf("My Account", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var header = ParsingHelper.GetTemplateHeader(xDoc, _ns);

                            if (header.IndexOf("Account Login", StringComparison.InvariantCultureIgnoreCase) > -1)
                            {
                                var service = new AccountService(Core);
                                template = new Template
                                {
                                    TemplateName = Config.TemplateEnum.Login,
                                    Service = service,
                                    Method = service.ParseLogin
                                };
                            }
                            else if (header.IndexOf("ORDER SUMMARY", StringComparison.InvariantCultureIgnoreCase) > -1)
                            {
                                var service = new AccountService(Core);
                                template = new Template
                                {
                                    TemplateName = Config.TemplateEnum.OrderDetail,
                                    Service = service,
                                    Method = service.ParseAccountOrderDetail
                                };
                            }
                            else if (breadCrumb.IndexOf("Order History", StringComparison.InvariantCultureIgnoreCase) > -1)
                            {
                                var service = new AccountService(Core);
                                template = new Template
                                {
                                    TemplateName = Config.TemplateEnum.OrderDetail,
                                    Service = service,
                                    Method = service.ParseAccountOrderDetail
                                };
                            }
                        }
                    }
                    else
                    {
                        var legend = xDoc.Descendants(_ns + "legend")
                                         .FirstOrNewXElement()
                                         .ElementValue();

                        if (legend.IndexOf("Select Shipping Method", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var service = new CheckoutService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutShipping,
                                Service = service,
                                Method = service.ParseShippingOptions
                            };
                        }
                        else if (legend.IndexOf("Order Summary", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var service = new CheckoutService(Core);
                            template = new Template
                            {
                                TemplateName = Config.TemplateEnum.CheckoutBilling,
                                Service = service,
                                Method = service.ParseUpdateSummary
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Errors.Add(e.Handle("ExtendedComRequest.GetTemplate",
                                      ErrorSeverity.FollowUp,
                                      ErrorType.Parsing));
            }

            return template;
        }


    }
}
