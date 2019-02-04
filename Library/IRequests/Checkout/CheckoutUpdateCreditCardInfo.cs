using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Enums;
using Library.Models;
using Library.Models.Requests;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using Newtonsoft.Json;
using Library.Helpers;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CheckoutUpdateCreditCardInfo", RequestType = typeof(CheckoutUpdateCreditCardInfoRequest), ResponseType = typeof(Response<CheckoutResponse>))]
    public class CheckoutUpdateCreditCardInfo : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private CheckoutUpdateCreditCardInfoRequest _request;
        public CheckoutUpdateCreditCardInfo(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CheckoutUpdateCreditCardInfo()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);
            _response = Communicate(communicationRequest);
            var result = ProcessResponse(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                _request = (CheckoutUpdateCreditCardInfoRequest) parameters;
                var url = string.Format("{0}{1}?creditCardUUID={2}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutUpdateCreditCardInfo, _request.CardId);
                if (_request.CheckoutResponse != null && !string.IsNullOrEmpty(_request.CheckoutResponse.CsrfToken))
                {
                    url = string.Format("{0}{1}?creditCardUUID={2}&csrf_token={3}", Config.Urls.SecureBaseUrl, Config.Urls.CheckoutUpdateCreditCardInfo, _request.CardId, _request.CheckoutResponse.CsrfToken);
                }
                
                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors)
                {
                    OverrideGetTemplate = true,
                    OverrideBlockXDocumentConversion = true
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutUpdateCreditCardInfo.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        public IResultResponse Communicate(ICommunicationRequest request)
        {
            try
            {
                var resultResponse = _core.RequestManager.Communicate(request);
                return resultResponse;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutUpdateCreditCardInfo.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CheckoutResponse>();
            result.resultset =  _request.CheckoutResponse;

            try
            {
                //check data first for valid json
                if (!response.RawData.Contains("<"))
                {
                    var dto = JsonConvert.DeserializeObject<SavedCardDTO>(response.RawData);
                    var cardInfo = result.resultset.PaymentInfo.CardInfo;
                    var savedCards = result.resultset.SavedCards;

                    cardInfo.Id = dto.MaskedNumber;
                    cardInfo.Type = dto.Type;
                    cardInfo.NameOnCard = dto.Holder;
                    cardInfo.Number = dto.MaskedFourDigit;
                    cardInfo.ExpirationMonth = dto.ExpirationMonth;
                    cardInfo.ExpirationYear = dto.ExpirationYear;
                    cardInfo.Cvv = string.Empty;

                    savedCards.ForEach(card => card.IsSelected = card.Value == _request.CardId);

                    if (!_errors.Any())
                    {
                        var session = new PaylessSession(_core);

                        session.SetCheckout(result.resultset);
                    }
                }
                else
                {
                    //this call returns the home page when you call it after the session has timed out
                    //this solution sucks, but works for now.

                    //tell the front end to time out
                    _errors.Add(new SiteError() { Source = "SessionTimeout" });
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CheckoutUpdateCreditCardInfo.ProcessResponse", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return result;
        }
    }
}
