using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Enums;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Services
{
    public class CheckoutService : IService
    {
        private readonly ICore _core;
        public List<SiteError> _errors { get; set; }
        private CheckoutResponse _checkout;
        private XDocument _xDoc;
        private XNamespace _ns;
        private readonly PaylessSession _session;

        public CheckoutService(ICore core)
        {
            _core = core;
            _session = new PaylessSession(core);
        }

        public IResponseBase Process(IResultResponse xResponse, IRequestParameter parameters, List<SiteError> errors)
        {
            _errors = errors;
            _checkout = _session.GetCheckout();
            _xDoc = xResponse.XDocument;

            if (_xDoc != null)
            {
                _ns = _xDoc.Root.GetDefaultNamespace();
            }

            
            var result = xResponse.Template.Method(xResponse, parameters);
            var parsedCheckout = (Response<CheckoutResponse>)result;
            if (parameters != null
                && parameters.GetType() == typeof(CheckoutBillingRequest)
                && ((CheckoutBillingRequest) parameters).IsApplyGiftCard)
            {
                _errors.AddRange(ParseGiftCardErrors());
            }
            else
            {
                if (!IsPayPalSuccess(parameters))
                {
                    _errors.AddRange(ParseGeneralErrors());
                }
                else
                {
                    parsedCheckout.resultset.PayPalInfo.IsSuccess = true;
                }
            }

            if (!_errors.Any(x => x.Type != ErrorType.UserActionRequired))
            {
                
                if (parameters == null || !parameters.GetType().Equals(typeof(CheckoutUpdateShippingMethodsRequest)))
                {
                    parsedCheckout.resultset.IsLoggedIn = IsLoggedIn();
                }
                
                parsedCheckout.resultset.Template = xResponse.Template.TemplateName.ToString();
                if (xResponse.Template.TemplateName.Equals(Config.TemplateEnum.CheckoutConfirmation))
                {
                    var newCheckout = new CheckoutResponse();
                    newCheckout.IsLoggedIn = parsedCheckout.resultset.IsLoggedIn;

                    _session.SetCheckout(newCheckout);
                }
                else
                {
                    _session.SetCheckout(parsedCheckout.resultset);
                }                

                if (_xDoc != null)
                {
                    parsedCheckout.resultset.TealiumDataBase = ParsingHelper.GetTealiumDataBase(_xDoc);
                    parsedCheckout.resultset.TealiumDataExtended = ParsingHelper.GetTealiumDataExtended(_xDoc);
                }
            }

            return result;
        }

        private bool IsPayPalSuccess(IRequestParameter parameters)
        {
            try
            {
                //success paypal requires you to have the following tokens. These values
                //were redirected directly from paypal. Thus, all conditions must be met.
                return (parameters != null
                            && parameters.GetType() == typeof(CheckoutPayPalSuccessRequest)
                            && !string.IsNullOrEmpty(((CheckoutPayPalSuccessRequest)parameters).DWControl)
                            && !string.IsNullOrEmpty(((CheckoutPayPalSuccessRequest)parameters).PayerId)
                            && !string.IsNullOrEmpty(((CheckoutPayPalSuccessRequest)parameters).Token));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IResponseBase ParseCheckoutBegin(IResultResponse xResponse, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                result.resultset.DWSecureKey = _xDoc.Descendants(_ns + "input")
                    .WhereAttributeEquals("name", "dwfrm_login_securekey")
                    .FirstOrNewXElement()
                    .AttributeValue("value");

                result.resultset.Cart.DWLoginParam = _xDoc.Descendants(_ns + "input")
                    .WhereAttributeContains("id", "dwfrm_login_username_")
                    .FirstOrNewXElement()
                    .AttributeValue("id");

                var form = _xDoc.Descendants(_ns + "form")
                    .WhereAttributeEquals("id", "dwfrm_login")
                    .FirstOrNewXElement();

                result.resultset.Cart.DWQuery = GetDWQuery(form);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseCheckoutBegin",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public IResponseBase ParseShipping(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                var mainContainer = _xDoc.Descendants(_ns + "form")
                    .WhereAttributeEquals("id", "dwfrm_singleshipping_shippingAddress")
                    .FirstOrNewXElement();

                var inputs = mainContainer.Descendants(_ns + "input")
                    .ToList();

                var selects = mainContainer.Descendants(_ns + "select")
                    .ToList();

                // Parse Shipping Address
                var address = result.resultset.ShippingInfo;
                address.FirstName = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_firstName");
                address.LastName = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_lastName");
                address.Address1 = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_address1");
                address.Address2 = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_address2").Replace("#","");
                address.City = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_city");
                address.State = GetSelectedValue(selects,
                    "dwfrm_singleshipping_shippingAddress_addressFields_states_state");
                address.Zip = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_zip");
                address.Phone = GetInputValue(inputs, "dwfrm_singleshipping_shippingAddress_addressFields_phone");

                // Parse IsBillingSameAsShipping
                var isBillingSameAsShipping = inputs.Find(x => x.AttributeValue("id").Contains("dwfrm_singleshipping_shippingAddress_useAsBillingAddress") && x.AttributeValue("checked").Equals("checked")) != null;

                // Parse Saved Addresses
                var savedAddresses = GetSavedAddressOptionsList(selects, "dwfrm_singleshipping_addressList");

                // Parse IsAddToAddressBook
                //var isAddToAddressBook = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_singleshipping_shippingAddress_addToAddressBook") && x.AttributeValue("checked").Equals("checked")) != null;
                var isAddToAddressBook = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_singleshipping_shippingAddress_addToAddressBook") && x.AttributeValue("checked").Equals("checked")) != null;


                var parsedOptions = ParseShippingOptions(_xDoc);
                result.resultset.ShippingOptions.Options = parsedOptions.Options;
                result.resultset.ShippingOptions.ShipToStorePrice = parsedOptions.ShipToStorePrice;
                result.resultset.ShippingOptions.ShipToStoreDescription = parsedOptions.ShipToStoreDescription;
                result.resultset.ShippingOptions.ShipToStoreLabel = parsedOptions.ShipToStoreLabel;
                result.resultset.ShippingOptions.ShippingSurchargeMessage = parsedOptions.ShippingSurchargeMessage;


                //var dwSecureKey = _xDoc.Descendants(_ns + "input").WhereAttributeEquals("name", "dwfrm_singleshipping_securekey").FirstOrNewXElement().AttributeValue("value");
                var dwSecureKey = _xDoc.Descendants(_ns + "input").WhereAttributeEquals("name", "dwfrm_singleshipping_securekey").FirstOrNewXElement().AttributeValue("value");
                // Parse DWQuesry
                var dwQuery = GetDWQuery(mainContainer);


                //var summaryContainer = _xDoc.Descendants(_ns + "table").WhereAttributeEquals("class", "order-totals-table").FirstOrNewXElement();
                var summaryContainer = _xDoc.Descendants(_ns + "table").WhereAttributeEquals("class", "order-totals-table").FirstOrNewXElement();

                if (summaryContainer != null)
                {
                    result.resultset.ReviewInfo.Summary = ParseSummary(summaryContainer);
                }
                else if (_checkout != null)
                {
                    result.resultset.ReviewInfo.Summary = GetCostsFromShippingOptions(_checkout);
                }

                result.resultset.PaymentInfo.BillingSameAsShipping = isBillingSameAsShipping;
                result.resultset.DWSecureKey = dwSecureKey;
                result.resultset.SavedAddresses = savedAddresses;
                result.resultset.IsAddToAddressBook = isAddToAddressBook;
                result.resultset.Cart.DWQuery = dwQuery;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseShipping",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private CartSummaryBase GetCostsFromShippingOptions(CheckoutResponse checkoutResponse)
        {
            var step1Url = string.Empty;
            try
            {

                var shippingInfo = checkoutResponse.ShippingInfo;
                var selectOption = GetSelectedShippingOption(checkoutResponse.ShippingOptions);
                var checkout = _session.GetCheckout();
                //step1Url = string.Format(Config.Urls.ShippingOptionsAPIPart1, shippingInfo.State, shippingInfo.Zip,shippingInfo.City, selectOption.Value);
                step1Url = string.Format(Config.Urls.ShippingOptionsAPIPart1, shippingInfo.State, shippingInfo.Zip, shippingInfo.City, selectOption.Value, checkout.CsrfToken);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.GetCostsFromShippingOptions",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }
            return GetCostsFromShippingOptions(step1Url);
        }

        private CartSummaryBase GetCostsFromShippingOptions(ShippingOption selectedOption)
        {
            var checkout = _session.GetCheckout();
            var step1Url = string.Format(Config.Urls.ShippingOptionsAPIPart1, "", "", "", selectedOption.Value, checkout.CsrfToken);
            return GetCostsFromShippingOptions(step1Url);
        }

        private CartSummaryBase GetCostsFromShippingOptions(string step1Url)
        {
            _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, step1Url, _core, _errors);
            _core.RequestManager.Communicate(_core.CommunicationRequest);

            _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET,
                Config.Urls.ShippingOptionsAPIPart2, _core, _errors);

            _core.CommunicationRequest.OptionalStreamReaderProcess = ParsingHelper.CustomStreamReaderProcess;

            var response = _core.RequestManager.Communicate(_core.CommunicationRequest);

            return ParseShippingOptionsOutput(response);
        }

        public CartSummaryBase ParseShippingOptionsOutput(IResultResponse response)
        {
            var cb = new CartSummaryBase();
            var options = response.XDocument
                .Descendants("table")
                .WhereAttributeEquals("class", "order-totals-table")
                .Descendants("tbody")
                .Descendants("tr")
                .Select(x =>
                {
                    var label = x.Descendants("td").Take(1).Select(a => a.Value).FirstOrDefault();

                    var price = x.Descendants("td").Skip(1).Take(1).Select(a => a.Value).FirstOrDefault();
                    if (!price.Contains("N/A"))
                    {
                        if (!price.Contains("FREE"))
                        {
                            //make it more readable:Degugging issues with double parse
                            //content is changing at payless. need to understand rasons.
                            var strTemplabel = label.Replace("Edit", "").Trim();
                            var strTempvalue = price.Replace("$", "").Replace(" ", "").Trim();
                            return new Price
                            {
                                Label = strTemplabel,
                                Value = double.Parse(strTempvalue)
                            };
                        }
                        return null;
                    }
                    return null;
                }).ToList();
            cb.Costs = options.Where(a => a != null && a.Label != "Order Total:").ToList();
            cb.Total = options.Where(a => a != null && a.Label == "Order Total:").FirstOrDefault();
            return cb;
        }

        public IResponseBase ParseShippingOptions(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            var request = (CheckoutUpdateShippingMethodsRequest) parameters;

            try
            {
                result.resultset.ShippingOptions = ParseShippingOptions(_xDoc);

                result.resultset.ShippingOptions.SelectedOption =
                    request.CheckoutResponse.ShippingOptions.SelectedOption;

                result.resultset.ReviewInfo.Summary = GetCostsFromShippingOptions(request.CheckoutResponse);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseShipping",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public IResponseBase ParseUpdateSummary(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                //var summaryContainer = _xDoc.Descendants(_ns + "table").WhereAttributeEquals("class", "order-totals-table").FirstOrNewXElement();
                var summaryContainer = _xDoc.Descendants(_ns + "table").WhereAttributeEquals("class", "order-totals-table").FirstOrNewXElement();

                result.resultset.ReviewInfo.Summary = ParseSummary(summaryContainer);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseShipping",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public IResponseBase ParseBilling(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                var mainContainer = _xDoc.Descendants(_ns + "form")
                    .WhereAttributeEquals("id", "dwfrm_billing")
                    .FirstOrNewXElement();

                var inputs = mainContainer.Descendants(_ns + "input")
                    .ToList();

                var selects = mainContainer.Descendants(_ns + "select")
                    .ToList();

                // Parse Billing Address
                var address = result.resultset.BillingInfo;
                address.FirstName = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_firstName");
                address.LastName = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_lastName");
                address.Address1 = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_address1");
                address.Address2 = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_address2").Replace("#","");
                address.City = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_city");
                address.State = GetSelectedValue(selects, "dwfrm_billing_billingAddress_addressFields_states_state");
                address.Zip = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_zip");
                address.Phone = GetInputValue(inputs, "dwfrm_billing_billingAddress_addressFields_phone");

                // Parse Saved Addresses
                var savedAddresses = GetSavedAddressOptionsList(selects, "dwfrm_billing_addressList");

                // Parse IsAddToAddressBook
                var isAddToAddressBook = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_billing_billingAddress_addToAddressBook") && x.AttributeValue("checked").Equals("checked")) != null;
                var isAddToEmailList = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_billing_billingAddress_addToEmailList") && x.AttributeValue("checked").Equals("checked")) != null;

                //var isAddToAddressBook = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_billing_billingAddress_addToAddressBook")&& x.AttributeValue("checked").Equals("checked")) != null;

                // Parse IsAddToEmailList
                //var isAddToEmailList = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_billing_billingAddress_addToEmailList")&& x.AttributeValue("checked").Equals("checked")) != null;

                // Parse Payment Info
                var paymentInfo = result.resultset.PaymentInfo;
                paymentInfo.CardInfo.NameOnCard = GetInputValue(inputs, "dwfrm_billing_paymentMethods_creditCard_owner");
                paymentInfo.CardInfo.Cvv = GetInputValue(inputs, "dwfrm_billing_paymentMethods_creditCard_cvn");
                //paymentInfo.CardInfo.ExpirationMonth = GetSelectedValue(selects,"dwfrm_billing_paymentMethods_creditCard_month");
                paymentInfo.CardInfo.ExpirationMonth = GetSelectedValue(selects, "dwfrm_billing_paymentMethods_creditCard_month");
                paymentInfo.CardInfo.ExpirationYear = GetSelectedValue(selects,"dwfrm_billing_paymentMethods_creditCard_year");


                paymentInfo.CardInfo.Months = GetOptionsList(selects, "dwfrm_billing_paymentMethods_creditCard_month");
                if (paymentInfo.CardInfo.Months.Count > 0)
                    paymentInfo.CardInfo.Months.First().Name =
                        paymentInfo.CardInfo.Months.First().Name.Replace("Select a ", string.Empty);

                paymentInfo.CardInfo.Years = GetOptionsList(selects, "dwfrm_billing_paymentMethods_creditCard_year");
                if (paymentInfo.CardInfo.Years.Count > 0)
                {
                    //Delete invalid expiration years (anything less than current year)
                    paymentInfo.CardInfo.Years.RemoveAll(year => IsValidExpirationYear(year.Value) == false);
                    paymentInfo.CardInfo.Years.First().Name =
                        paymentInfo.CardInfo.Years.First().Name.Replace("Select a ", string.Empty);
                }

                paymentInfo.AvailableCardTypes = GetOptionsList(selects, "dwfrm_billing_paymentMethods_creditCard_type");

                if (!string.IsNullOrEmpty(paymentInfo.CardInfo.NameOnCard))
                {
                    var selectedType = paymentInfo.AvailableCardTypes.Find(x => x.IsSelected);
                    if (selectedType != null)
                    {
                        paymentInfo.CardInfo.Type = selectedType.Value;
                    }
                }
                else
                {
                    paymentInfo.CardInfo.Type = string.Empty;
                }

                var maskedFourDigit = GetInputValue(inputs, "dwfrm_billing_paymentMethods_creditCard_maskedFourDigit");
                var creditCardNumber = GetInputValue(inputs, "dwfrm_billing_paymentMethods_creditCard_number");

                if (!string.IsNullOrEmpty(maskedFourDigit))
                {
                    paymentInfo.CardInfo.Number = maskedFourDigit;
                    paymentInfo.CardInfo.Id = creditCardNumber;
                }
                else
                {
                    paymentInfo.CardInfo.Number = creditCardNumber;
                    paymentInfo.CardInfo.Id = string.Empty;
                }

                // Parse No Payment Needed Flag
                var isNoPaymentNeeded = inputs.Find(x => x.AttributeValue("id").Equals("noPaymentNeeded")
                                                         && x.AttributeValue("value").Equals("true")) != null;

                // Parse Saved Cards
                var savedCards = GetOptionsList(selects, "creditCardList");

                // Parse Secure Key
                //var dwSecureKey = _xDoc.Descendants(_ns + "input").WhereAttributeEquals("name", "dwfrm_billing_securekey").FirstOrNewXElement().AttributeValue("value");
                var dwSecureKey = _xDoc.Descendants(_ns + "input").WhereAttributeEquals("name", "dwfrm_billing_securekey").FirstOrNewXElement().AttributeValue("value");

                // Parse IsSaveThisCard
                //var isSaveThisCard = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_billing_paymentMethods_creditCard_saveCard")&& x.AttributeValue("checked").Equals("checked")) != null;
                var isSaveThisCard = inputs.Find(x => x.AttributeValue("id").Equals("dwfrm_billing_paymentMethods_creditCard_saveCard") && x.AttributeValue("checked").Equals("checked")) != null;
                
                

                // Parse Gift Cards Info
                var gcNumber = GetInputValue(inputs, "dwfrm_billing_giftCardNum");

                // Parse Captcha Href and Secure Key
                var captchaHref = _xDoc.Descendants(_ns + "iframe")
                    .FirstOrNewXElement()
                    .AttributeValue("src");

                var dwCaptchaSecureKey = _xDoc.Descendants(_ns + "input")
                    .WhereAttributeEquals("name", "dwfrm_ReCaptchaTest_securekey")
                    .FirstOrNewXElement()
                    .AttributeValue("value");

                // Parse DWQuesry
                var dwQuery = GetDWQuery(mainContainer);

                // Parse Review Shipping Info
                var shipping = _xDoc.Descendants(_ns + "div")
                    .WhereAttributeContains("class", "mini-shipment")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "details")
                    .FirstOrNewXElement()
                    .Elements()
                    .Select(x => x.ElementValue())
                    .ToList();

                // Parse Summary
                var summaryContainer = _xDoc.Descendants(_ns + "table")
                    .WhereAttributeEquals("class", "order-totals-table")
                    .FirstOrNewXElement();

                if (summaryContainer != null)
                {
                    result.resultset.ReviewInfo.Summary = ParseSummary(summaryContainer);
                }
                else if (_checkout != null)
                {
                    //this is probably unused
                    result.resultset.ReviewInfo.Summary = GetCostsFromShippingOptions(_checkout);
                }

                result.resultset.PaymentMethods = ParsePaymentMethods(mainContainer);
                result.resultset.IsNoPaymentNeeded = isNoPaymentNeeded;
                result.resultset.DWSecureKey = dwSecureKey;
                result.resultset.CaptchaHref = captchaHref;
                result.resultset.DWCaptchaSecureKey = dwCaptchaSecureKey;
                result.resultset.SavedAddresses = savedAddresses;
                result.resultset.IsAddToEmailList = isAddToEmailList;
                result.resultset.IsAddToAddressBook = isAddToAddressBook;
                result.resultset.SavedCards = savedCards;
                result.resultset.IsSaveThisCard = isSaveThisCard;
                result.resultset.GiftCard = new GiftCard {GiftCardNumber = gcNumber};
                result.resultset.AppliedGiftCards = ParseAppliedGiftCards(mainContainer);
                result.resultset.ApplyCouponMessage = string.Empty;
                result.resultset.Cart.DWQuery = dwQuery;
                result.resultset.ReviewInfo.Shipping = shipping;
                result.resultset.Cart.GoogleWalletInfo = ParsingHelper.GetBillingGoogleWalletInfo(response,
                    result.resultset.Cart);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseBilling",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        //SPAY-105 We are grabbing all values for expiration year, so I added this to aid my linq expression to delete in valid years
        //if the value is string empty, equal to, or greater than current year this returns true, otherwise, false
        private bool IsValidExpirationYear(string year)
        {
            var isValid = new bool();
            if ((year == "") || (int.Parse(year) >= DateTime.Now.Year))
                isValid = true;
            else
                isValid = false;
            return isValid;
        }

        //cperez:new gift-card model is not working due to new tags.
        //rollingback to original state since giftcard pin feature will not be working.
        private List<GiftCard> ParseAppliedGiftCards(XElement container)
        {
            var result = new List<GiftCard>();


            result = container.Descendants(_ns + "div")
                .WhereAttributeEquals("class", "success giftcert-pi")
                .WhereAttributeContains("id", "gc")
                .Select(x =>
                {
                    var message = x.ElementValue()
                        .Replace("Remove", string.Empty);

                    var removeHref = x.Elements(_ns + "a")
                        .FirstOrNewXElement()
                        .AttributeValue("href");

                    var discount = new Price(message);

                    var giftCardNumber = x.AttributeValue("id")
                        .Replace("gc-", string.Empty);

                    return new GiftCard
                    {
                        Message = message,
                        RemoveHref = removeHref,
                        Discount = discount,
                        GiftCardNumber = giftCardNumber
                    };
                }).ToList();

            return result;
        }

        private List<Option> ParsePaymentMethods(XElement container)
        {
            var result = new List<Option>();

            var selectedValue = container.Descendants(_ns + "input")
                .WhereAttributeEquals("id", "GoogleWalletEdit")
                .FirstOrNewXElement().AttributeValue("value");

            result = container.Descendants(_ns + "div")
                .WhereAttributeEquals("class", "payment-method-options")
                .FirstOrNewXElement()
                .Descendants(_ns + "div")
                .WhereAttributeEquals("class", "form-row")
                .Where(x => !x.Descendants(_ns + "input")
                    .FirstOrNewXElement()
                    .AttributeValue("disabled")
                    .Equals("disabled"))
                .Select(x =>
                {
                    var name = x.ElementValue()
                        .Replace(":", string.Empty);

                    var input = x.Elements(_ns + "input")
                        .FirstOrNewXElement();

                    var value = input.AttributeValue("value");

                    return new Option
                    {
                        Name = name,
                        Value = value,
                        IsSelected = value == selectedValue
                    };
                })
                .ToList();

            return result;
        }

        public IResponseBase ParseAddressValidation(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseAddressValidation",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public IResponseBase ParseReview(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                var mainContainer = _xDoc.Descendants(_ns + "div")
                    .WhereAttributeEquals("id", "primary")
                    .FirstOrNewXElement();

                // Parse Review Shipping Info
                var shipping = mainContainer.Descendants(_ns + "div")
                    .WhereAttributeContains("class", "mini-shipment")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "details")
                    .FirstOrNewXElement()
                    .Elements()
                    .Select(x => x.ElementValue())
                    .ToList();

                // Parse Review Billing Info
                var billing = mainContainer.Descendants(_ns + "div")
                    .WhereAttributeContains("class", "mini-billing-address")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "details")
                    .FirstOrNewXElement()
                    .Elements()
                    .Select(x => x.ElementValue())
                    .ToList();

                // Parse Review Payment Method
                var paymentContainer = mainContainer.Descendants(_ns + "div")
                    .WhereAttributeContains("class", "mini-payment-instrument")
                    .Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "details")
                    .FirstOrNewXElement();

                var payment = paymentContainer.Elements()
                    .Select(x => x.ElementValue())
                    .ToList();

                if (payment.Any())
                {
                    while (payment.FirstOrDefault() == string.Empty)
                    {
                        payment.RemoveAt(0);
                    }
                }

                result.resultset.ReviewInfo.IsGoogleWallet = paymentContainer.Elements(_ns + "img")
                    .WhereAttributeEquals("alt", "Google Wallet")
                    .Any();
                if (result.resultset.ReviewInfo.IsGoogleWallet)
                {
                    result.resultset.Cart.GoogleWalletInfo = ParsingHelper.GetReviewGoogleWalletInfoFrom(response,
                        result.resultset.Cart);
                }


                var summaryContainer = mainContainer.Descendants(_ns + "table")
                    .WhereAttributeEquals("class", "order-totals-table")
                    .FirstOrNewXElement();


                if (string.IsNullOrWhiteSpace(summaryContainer.Value))
                {
                    var secondaryContainer = _xDoc.Descendants(_ns + "div")
                        .WhereAttributeEquals("id", "spsecondary")
                        .FirstOrNewXElement();


                    summaryContainer = secondaryContainer.Descendants(_ns + "table")
                        .WhereAttributeEquals("class", "order-totals-table")
                        .FirstOrNewXElement();
                }

                if (summaryContainer != null)
                {
                    result.resultset.ReviewInfo.Summary = ParseSummary(summaryContainer);
                }
                else if (parameters.GetType() == typeof(CheckoutBillingRequest))
                {
                    var theParams = (CheckoutBillingRequest) parameters;

                    result.resultset.ReviewInfo.Summary = GetCostsFromShippingOptions(theParams.CheckoutResponse);
                }

                result.resultset.ReviewInfo.Shipping = shipping;
                result.resultset.ReviewInfo.Billing = billing;
                result.resultset.ReviewInfo.Payment = payment;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseReview",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }


        public IResponseBase ParseConfirmation(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                var dateStr = _xDoc.Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "order-date")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "span")
                    .WhereAttributeEquals("class", "value")
                    .FirstOrNewXElement()
                    .ElementValue();

                var orderNumber = _xDoc.Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "order-number")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "span")
                    .WhereAttributeEquals("class", "value")
                    .FirstOrNewXElement()
                    .ElementValue();

                var status = _xDoc.Descendants(_ns + "div")
                    .WhereAttributeEquals("class", "shipping-status")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "span")
                    .WhereAttributeEquals("class", "value")
                    .FirstOrNewXElement()
                    .ElementValue();

                // Criteo Customer Id
                var criteoCustomerId = ParsingHelper.GetCriteoCustomerId(response);

                result.resultset.OrderDetail.Message = dateStr;
                result.resultset.OrderDetail.OrderConfirmationNumber = orderNumber;
                result.resultset.OrderDetail.Status = status;
                result.resultset.Cart.CriteoCustomerId = criteoCustomerId;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseConfirmation",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public List<SiteError> ParseGeneralErrors()
        {
            var result = new List<SiteError>();

            try
            {
                if (_xDoc != null)
                {
                    result = _xDoc.Descendants(_ns + "div")
                        .Where(z => z.AttributeValue("class") == "error" || z.AttributeValue("class") == "error-form")
                        .Select(x => new SiteError
                        {
                            Message = new ErrorMessage(x.ElementValue(), string.Empty),
                            Severity = ErrorSeverity.UserActionRequired,
                            Type = ErrorType.UserActionRequired
                        })
                        .ToList();

                    result.AddRange(_xDoc.Descendants(_ns + "span")
                        .Where(z => z.AttributeValue("class").Contains("error"))
                        .Select(x => new SiteError
                        {
                            Message = new ErrorMessage(x.ElementValue(), string.Empty),
                            Severity = ErrorSeverity.UserActionRequired,
                            Type = ErrorType.UserActionRequired
                        })
                        .ToList());
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "MadServ.CartService.ParseGeneralErrors",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public List<SiteError> ParseGiftCardErrors()
        {
            var result = new List<SiteError>();

            try
            {
                result.AddRange(_xDoc.Descendants(_ns + "div")
                    .WhereAttributeContains("class", "redemption")
                    .Descendants(_ns + "span")
                    .Where(z => z.AttributeValue("class").Contains("error"))
                    .Select(x => new SiteError
                    {
                        Message = new ErrorMessage(x.ElementValue(), string.Empty),
                        Severity = ErrorSeverity.UserActionRequired,
                        Type = ErrorType.UserActionRequired
                    })
                    .ToList());
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "MadServ.CartService.ParseGeneralErrors",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public IResponseBase ParsePayPalRedirect(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                //result.resultset.PayPalInfo.PayPalRedirectUrl = response.ResponseHeaders["Location"];
                var startIndex = response.RawData.IndexOf("url=");
                var endIndex = response.RawData.IndexOf("><meta http-equiv=\"Robots\"");
                result.resultset.PayPalInfo.PayPalRedirectUrl =
                    response.RawData.Substring(startIndex, endIndex - startIndex)
                        .Replace("url=", "")
                        .Replace("\"", "")
                        .Trim();
                //.Replace("cmd=_express-checkout", "cmd=_express-checkout-mobile")
                //.Replace("&&force_web=true", string.Empty);

                result.resultset.PayPalInfo.IsBillingInfoStep = parameters != null &&
                                                                parameters.GetType()
                                                                    .Equals(typeof(CheckoutBillingRequest));
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParsePayPalRedirect",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        public IResponseBase ParsePayPalError(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };
            try
            {
                var errorMessage = "";

                _errors.Add(new SiteError
                {
                    Type = ErrorType.UserActionRequired,
                    Severity = ErrorSeverity.UserActionRequired,
                    Message = new ErrorMessage(errorMessage, errorMessage)
                });
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParsePayPal",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private ShippingOption GetSelectedShippingOption(ShippingOptions shippingOptions)
        {
            ShippingOption selectedOption = null;

            if (shippingOptions != null)
            {
                selectedOption = shippingOptions.Options.Where(a => a.IsSelected).FirstOrDefault();
            }

            if (selectedOption == null)
            {
                selectedOption = new ShippingOption {Value = "1001"};
            }

            return selectedOption;
        }

        private ShippingOptions ParseShippingOptions(XDocument xDoc)
        {
            var result = new ShippingOptions();

            try
            {
                var options = xDoc.Descendants(_ns + "input")
                    .WhereAttributeContains("name", "shippingMethodID")
                    .Select(radio =>
                    {
                        var label = radio
                            .Parent
                            .Parent
                            .Descendants(_ns + "label").FirstOrDefault().ElementValue();

                        var value = radio.AttributeValue("value");

                        var description = radio.Parent.Parent.Descendants(_ns + "div")
                            .WhereAttributeEquals("class", "form-caption")
                            .FirstOrNewXElement()
                            .ElementValue();

                        var isSelected = radio.AttributeValue("checked") == "checked";

                        return new ShippingOption
                        {
                            Label = label,
                            Value = value,
                            Description = description,
                            IsSelected = isSelected,
                            Price = new Price(label)
                        };
                    }).ToList();


                //original code
                var shipToStoreOptions = options.FindAll(x => x.Value.StartsWith("1004"));
                if (shipToStoreOptions.Count <= 0)
                {
                    //SPAY-96: cperez- TEMP HACK DUE TO CHECKOUT
                    //Client removed the shipto store option. this cause errors on UI.
                    //Doing null safe will not work due to UI. the easiest way for now
                    //is to add the dummy option to keep the flow as normal. the code below
                    //allows the UI to work, the ship to store option will just not work.
                    //CLIENT IS REVIEWING a REVERT BACK.
                    var tempShipToOption = new ShippingOption();
                    tempShipToOption.Description = "Ship to Store";
                    tempShipToOption.IsSelected = false;
                    tempShipToOption.Label = "Ship to Store";
                    tempShipToOption.Price = new Price
                    {
                        Currency = "USD",
                        IsStrikeThrough = false,
                        SortOrder = 0,
                        Value = 4.99
                    };
                    tempShipToOption.Value = "1004";
                    options.Add(tempShipToOption);
                    shipToStoreOptions = options.FindAll(x => x.Value.StartsWith("1004"));
                }


                if (shipToStoreOptions.Any(x => x.IsSelected))
                {
                    result.SelectedOption = Config.Constants.ShipToStore;
                }

                options.RemoveAll(x => x.Value.StartsWith("1004"));

                var storeOption = shipToStoreOptions.FirstOrDefault();

                result.ShipToStorePrice = storeOption.Price;
                result.ShipToStoreDescription = storeOption.Description;
                result.ShipToStoreLabel = storeOption.Label;


                var surcharge = xDoc.Descendants(_ns + "p").WhereAttributeContains("class", "shipping-method-surcharge").FirstOrDefault();
                if (surcharge != null)
                {
                    result.ShippingSurchargeMessage = surcharge.Value;
                }

                result.Options = options;

                //result.ShipToStoreZip = GetInputValue(inputs, "address");
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.ParseShippingOptions",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private string GetInputValue(List<XElement> inputs, string id)
        {
            var result = string.Empty;

            try
            {
                var match = inputs.Find(x => x.AttributeValue("id").Equals(id));

                if (match != null)
                {
                    result = match.AttributeValue("value");
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.GetInputValue",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private string GetSelectedValue(List<XElement> selects, string id)
        {
            var result = string.Empty;

            try
            {
                var match = selects.Find(x => x.AttributeValue("id").Contains(id));

                if (match != null)
                {
                    result = match.Descendants(_ns + "option").WhereAttributeEquals("selected", "selected").FirstOrNewXElement().AttributeValue("value");
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.GetSelectedValue",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private List<Option> GetOptionsList(List<XElement> selects, string id)
        {
            var result = new List<Option>();

            try
            {
                var match = selects.Find(x => x.AttributeValue("id").Contains(id));

                if (match != null)
                {
                    result = match.Descendants(_ns + "option")
                        .Select(x =>
                        {
                            var name = x.ElementValue();

                            var value = x.AttributeValue("value");

                            var isSelected = x.AttributeValue("selected")
                                .Equals("selected");

                            return new Option
                            {
                                Name = name,
                                Value = value,
                                IsSelected = isSelected
                            };
                        })
                        .ToList();

                    if (result != null && result.Any() && result.Find(x => x.IsSelected) == null)
                    {
                        result.FirstOrDefault().IsSelected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.GetOptionsList",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private List<SavedAddressOption> GetSavedAddressOptionsList(List<XElement> selects, string id)
        {
            var result = new List<SavedAddressOption>();

            try
            {
                var match = selects.Find(x => x.AttributeValue("id").Contains(id));

                if (match != null)
                {
                    result = match.Descendants(_ns + "option")
                        .Select(x =>
                        {
                            var name = x.ElementValue();

                            var value = x.AttributeValue("value");

                            var isSelected = x.AttributeValue("selected")
                                .Equals("selected");

                            var json = x.AttributeValue("data-address");

                            var accountAddress = new AccountAddress(json);

                            return new SavedAddressOption
                            {
                                Name = name,
                                Value = value,
                                IsSelected = isSelected,
                                AccountAddress = accountAddress
                            };
                        })
                        .ToList();

                    if (result != null && result.Any() && result.Find(x => x.IsSelected) == null)
                    {
                        result.FirstOrDefault().IsSelected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "CheckoutService.GetSavedAddressOptionsList",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private CartSummaryBase ParseSummary(XElement container)
        {
            var result = new CartSummaryBase();

            try
            {
                var total = container.Descendants(_ns + "tr")
                    .WhereAttributeEquals("class", "order-total")
                    .FirstOrNewXElement()
                    .ElementValue();

                var subTotal = container.Descendants(_ns + "tr")
                    .WhereAttributeEquals("class", "order-subtotal")
                    .FirstOrNewXElement()
                    .ElementValue();

                var discount = container.Descendants(_ns + "tr")
                    .WhereAttributeEquals("class", "order-discount discount")
                    .FirstOrNewXElement()
                    .ElementValue();

                var shipping = container.Descendants(_ns + "tr")
                    .WhereAttributeContains("class", "order-shipping")
                    .FirstOrNewXElement()
                    .ElementValue();

                var shippingDiscount = container.Descendants(_ns + "tr")
                    .WhereAttributeEquals("class", "order-shipping-discount discount")
                    .FirstOrNewXElement()
                    .ElementValue();

                var tax = container.Descendants(_ns + "tr")
                    .WhereAttributeEquals("class", "order-sales-tax")
                    .FirstOrNewXElement()
                    .ElementValue();

                result.Total = new Price(total) {Label = "Order Total"};

                result.Costs = new List<Price>();

                result.Costs.Add(new Price(subTotal) {Label = "Subtotal"});

                if (!string.IsNullOrEmpty(discount))
                    result.Costs.Add(new Price(discount) {Label = "Order Discount"});

                if (!shipping.Contains("N/A"))
                    result.Costs.Add(new Price(shipping) {Label = "Shipping"});

                if (!string.IsNullOrEmpty(shippingDiscount))
                    result.Costs.Add(new Price(shippingDiscount) {Label = "Shipping Discount"});

                if (!tax.Contains("N/A"))
                    result.Costs.Add(new Price(tax) {Label = "Sales Tax"});
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "MadServ.CheckoutService.ParseSummary",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private List<CartItem> ParseCheckoutItems(IEnumerable<XElement> container)
        {
            var result = new List<CartItem>();

            try
            {
                if (container != null)
                {
                    foreach (var xEl in container)
                    {
                        var src = xEl.Descendants(_ns + "td")
                            .WhereAttributeEquals("class", "item-image")
                            .FirstOrNewXElement()
                            .Elements(_ns + "img")
                            .FirstOrNewXElement()
                            .AttributeValue("src");

                        var detailsContainer = xEl.Descendants(_ns + "div")
                            .WhereAttributeEquals("class", "product-list-item")
                            .FirstOrNewXElement();

                        var name = detailsContainer.Elements(_ns + "div")
                            .WhereAttributeEquals("class", "name")
                            .FirstOrNewXElement()
                            .ElementValue();

                        var href = detailsContainer.Elements(_ns + "div")
                            .WhereAttributeEquals("class", "name")
                            .FirstOrNewXElement()
                            .Elements(_ns + "a")
                            .FirstOrNewXElement()
                            .AttributeValue("href")
                            .Replace(Config.Urls.BaseUrl, string.Empty);

                        var productId = detailsContainer.Elements(_ns + "div")
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

                        var qtyStr = xEl.Descendants(_ns + "td")
                            .WhereAttributeEquals("class", "item-quantity")
                            .FirstOrNewXElement()
                            .Elements(_ns + "span")
                            .FirstOrNewXElement()
                            .ElementValue();
                        var qty = 0;
                        int.TryParse(qtyStr, out qty);

                        var totalPrice = xEl.Descendants(_ns + "td")
                            .WhereAttributeEquals("class", "item-total")
                            .FirstOrNewXElement()
                            .ElementValue();

                        var promoMessages = detailsContainer.Elements(_ns + "div")
                            .WhereAttributeContains("class", "promo")
                            .Select(x => x.ElementValue())
                            .ToList();

                        result.Add(new CartItem
                        {
                            Image = new Image {Src = src},
                            Name = name,
                            Href = href,
                            ProductId = productId,
                            Color = color,
                            Size = size,
                            Width = width,
                            Design = design,
                            GCValue = gcValue,
                            Quantity = qty,
                            TotalPrice = new Price(totalPrice),
                            Notes = promoMessages
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "MadServ.CheckoutService.ParseCheckoutItems",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private string GetDWQuery(XElement container)
        {
            var result = string.Empty;

            try
            {
                var action = container.AttributeValue("action");
                var begin = action.IndexOf("?dwcont=");

                if (begin > -1)
                {
                    result = action.Substring(begin, action.Length - begin);
                }
                else
                {
                    var regex = new Regex(@"(?<dwcont>C[\d]{9})");
                    result = string.Format("?dwcont={0}", regex.Match(action).Groups["dwcont"].Value);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                    "MadServ.CheckoutService.GetDWQuery",
                    ErrorSeverity.FollowUp,
                    ErrorType.Parsing
                    ));
            }

            return result;
        }

        private bool IsLoggedIn()
        {
            var result = false;

            try
            {
                result = _xDoc.Descendants(_ns + "ul")
                    .WhereAttributeEquals("class", "menu-utility-user")
                    .FirstOrNewXElement()
                    .Descendants(_ns + "a")
                    .WhereAttributeContains("title", "Logout")
                    .Any();
            }
            catch
            {
            }

            return result;
        }

        public Response<CheckoutResponse> ProcessCoupon(IResultResponse response, IRequestParameter parameters)
        {
            var theParams = (CartApplyCouponRequest) parameters;
            var result = new Response<CheckoutResponse>
            {
                resultset = _session.GetCheckout()
            };
            result.resultset.ReviewInfo.Summary = ParseShippingOptionsOutput(response);
            result.template = "CheckoutBilling";
            return result;
        }
    }
}
