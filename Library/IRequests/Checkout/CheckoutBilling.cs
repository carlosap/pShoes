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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutBilling", RequestType = typeof(CheckoutBillingRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutBilling : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }

        private bool _isPayPal;
        private CheckoutResponse _checkout;

        public CheckoutBilling()
        {
            _errors = new List<SiteError>();
        }
        public CheckoutBilling(ICore core)
            : this()
        {
            _core = core;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl((CheckoutBillingRequest)parameters);

            //4/23/2015, JAY: they added a new McAfee symbol which contained malformed html
            //CustomStreamReaderProcess removes the junk; commenting this out for now because they supposedly fixed it.
            //communicationRequest.OptionalStreamReaderProcess = ParsingHelper.CustomStreamReaderProcess;

            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(CheckoutBillingRequest request)
        {
            try
            {
                if (request.CheckoutResponse != null)
                {
                    _checkout = request.CheckoutResponse;
                    var billingInfo = _checkout.BillingInfo;
                    var cardInfo = _checkout.PaymentInfo.CardInfo;
                    var isPrepopulatedCard = !string.IsNullOrEmpty(cardInfo.Id);
                    var selectedSavedAddress = _checkout.SavedAddresses.Where(x => x.IsSelected).FirstOrDefault() ?? new SavedAddressOption();
                    var selectedSavedCard = _checkout.SavedCards.Where(x => x.IsSelected).FirstOrDefault() ?? new Option();

                    var url = string.Format("{0}{1}{2}&csrf_token={3}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutBillingPost, _checkout.Cart.DWQuery,_checkout.CsrfToken);
                    //cpere: rollback due to gift-card issues.
                    //var url = string.Format("{0}{1}/{2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutBillingPost, _checkout.Cart.DWQuery.Replace("?dwcont=", ""));

                    var postSb = new StringBuilder();

                    postSb.AppendFormat("dwfrm_billing_save={0}", "true");
                    if (_checkout.IsLoggedIn)
                        postSb.AppendFormat("&dwfrm_billing_addressList={0}", HttpUtility.UrlEncode(selectedSavedAddress.Value));

                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_firstName={0}", HttpUtility.UrlEncode(billingInfo.FirstName));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_lastName={0}", HttpUtility.UrlEncode(billingInfo.LastName));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_address1={0}", HttpUtility.UrlEncode(billingInfo.Address1));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_address2={0}", HttpUtility.UrlEncode(billingInfo.Address2));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_country={0}", "US");
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_city={0}", HttpUtility.UrlEncode(billingInfo.City));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_states_state={0}", HttpUtility.UrlEncode(billingInfo.State));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_zip={0}", HttpUtility.UrlEncode(billingInfo.Zip));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_addressFields_phone={0}", HttpUtility.UrlEncode(billingInfo.Phone));
                    postSb.AppendFormat("&dwfrm_billing_billingAddress_email_emailAddress={0}", HttpUtility.UrlEncode(_checkout.Email));
                    if (_checkout.IsAddToAddressBook)
                        postSb.Append("&dwfrm_billing_billingAddress_addToAddressBook=true");
                    if (_checkout.IsAddToEmailList)
                        postSb.Append("&dwfrm_billing_billingAddress_addToEmailList=true");
                    if (_checkout.IsNoPaymentNeeded)
                    {
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_selectedPaymentMethodID={0}", "GIFT_CERTIFICATE");
                        postSb.Append("&noPaymentNeeded=true");
                    }
                    else
                    {
                        var selectedPaymentMethod = _checkout.PaymentMethods.Find(x => x.IsSelected) ?? new Option();
                        _isPayPal = selectedPaymentMethod.Value == "PayPal";

                        if (_isPayPal)
                        {
                            postSb.AppendFormat("&EnvironmentID={0}", EnvironmentHelper.GetEnvironmentId(_core.Context.Request));
                        }

                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_selectedPaymentMethodID={0}", selectedPaymentMethod.Value);
                        if (_checkout.IsLoggedIn)
                            postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCardList={0}", HttpUtility.UrlEncode(selectedSavedCard.Value));
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_owner={0}", HttpUtility.UrlEncode(cardInfo.NameOnCard));
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_type={0}", !string.IsNullOrEmpty(cardInfo.Type) ? cardInfo.Type : _checkout.PaymentInfo.AvailableCardTypes.FirstOrDefault().Value);
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_maskedFourDigit={0}", isPrepopulatedCard ? cardInfo.Number : string.Empty);
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_number={0}", isPrepopulatedCard ? cardInfo.Id : cardInfo.Number);
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_month={0}", cardInfo.ExpirationMonth);
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_year={0}", cardInfo.ExpirationYear);
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_isSubscription={0}", isPrepopulatedCard.ToString());
                        postSb.AppendFormat("&dwfrm_billing_paymentMethods_creditCard_cvn={0}", cardInfo.Cvv);
                    }
                    if (_checkout.IsLoggedIn && _checkout.IsSaveThisCard)
                        postSb.Append("&dwfrm_billing_paymentMethods_creditCard_saveCard=true");
                    postSb.Append("&dwfrm_billing_paymentMethods_selectedPaymentMethodID=");
                    postSb.AppendFormat("&dwfrm_billing_securekey={0}", _checkout.DWSecureKey);
                    postSb.Append("&dwfrm_billing_couponCode=");
                    postSb.AppendFormat("&dwfrm_billing_giftCardNum={0}", _checkout.GiftCard.GiftCardNumber);
                    postSb.AppendFormat("&recaptcha_challenge_field={0}", _checkout.CaptchaChalange);
                    postSb.AppendFormat("&recaptcha_response_field={0}", _checkout.CaptchaResponse);
                    postSb.AppendFormat("&dwfrm_ReCaptchaTest_securekey={0}", _checkout.DWCaptchaSecureKey);

                    if (request.IsApplyGiftCard)
                        postSb.AppendFormat("&dwfrm_billing_redeemGiftCert={0}", "Apply");
                    else
                        postSb.AppendFormat("&dwfrm_billing_save={0}", "Next%3A+Review+Your+Order");


                    postSb.AppendFormat("&dwfrm_billing_giftCardPin={0}", _checkout.GiftCard.GiftCardPinNumber);
                    



                    _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                    {
                        OverridePostData = postSb.ToString(),
                        OverrideStopAutoRedirects = _isPayPal,
                        OptionalReturnResponseHeaders = _isPayPal,
                        OverrideBlockXDocumentConversion = _isPayPal,
                    };
                }
                else
                {
                    var url = string.Format("{0}{1}?EnvironmentID={2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutBillingGet, EnvironmentHelper.GetEnvironmentId(_core.Context.Request));
                    _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBilling.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CheckoutBilling.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            IResponseBase result = new Response<CheckoutResponse>
            {
                resultset = _checkout
            };

            try
            {
                if (response.Template.Service != null)
                {
                    result = response.Template.Service.Process(response, parameters, _errors);
                }
                else
                {
                    _errors.Add(new SiteError
                    {
                        Message = new ErrorMessage(Config.Constants.GenericError, Config.Constants.GenericError),
                        Severity = ErrorSeverity.UserActionRequired,
                        Type = ErrorType.UserActionRequired
                    });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutBeginPayPal.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }

            return result;
        }
    }
}
