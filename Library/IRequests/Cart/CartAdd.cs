using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;


namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CartAdd", RequestType = typeof(CartAddRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CartAdd : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CartAdd(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CartAdd()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);

            //4/23/2015, JAY: they added a new McAfee symbol which contained malformed html
            //CustomStreamReaderProcess removes the junk; commenting this out for now because they supposedly fixed it.
            //communicationRequest.OptionalStreamReaderProcess = ParsingHelper.CustomStreamReaderProcess;

            _response = Communicate(communicationRequest);
            var result = ProcessCart(_response, parameters);

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            string url = Config.Urls.SecureBaseUrl + Config.Urls.CartAdd;

            try
            {
                var request = (CartAddRequest)parameters;
                var postSb = new StringBuilder();

                postSb.AppendFormat("Quantity={0}", request.Quantity);
                postSb.Append("&cartAction=add");
                postSb.Append("&omnitureAddToCartType=Product+View");
                postSb.AppendFormat("&pid={0}", request.Sku);

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.POST, url, _core, _errors)
                {
                    OverridePostData = postSb.ToString()
                };
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CartAdd.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CartAdd.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
