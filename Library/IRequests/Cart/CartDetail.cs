using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Enums;
using Library.Helpers;
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
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CartDetail", RequestType = typeof(CartDetailRequest), ResponseType = typeof(Response<CartResponse>))]
    public class CartDetail : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CartDetail(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public CartDetail(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public CartDetail()
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

            try
            {
                StringBuilder urlSb = new StringBuilder();
                    
                urlSb.Append(Config.Urls.SecureBaseUrl);
                urlSb.Append(Config.Urls.CartDetail);

                if (parameters != null)
                {
                    var request = (CartDetailRequest)parameters;
                    var query = new List<string>();

                    if (!string.IsNullOrEmpty(request.SourceCode))
                    {
                        query.Add(string.Format("src={0}", request.SourceCode));
                    }

                    if (!string.IsNullOrEmpty(request.CampaignId))
                    {
                        query.Add(string.Format("ocid={0}", request.CampaignId));
                    }

                    if (query.Count > 0)
                    {
                        urlSb.AppendFormat("?{0}", string.Join("&", query));
                    }

                }
                else
                {
                    var query = new List<string>();
                    query.Add(string.Format("EnvironmentID={0}", EnvironmentHelper.GetEnvironmentId(_core.Context.Request)));

                    if (query.Count > 0)
                    {
                        urlSb.AppendFormat("?{0}", string.Join("&", query));
                    }
                }

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, urlSb.ToString(), _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CartDetail.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
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
                _errors.Add(ex.Handle("CartDetail.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        public IResponseBase ProcessCart(IResultResponse response, IRequestParameter parameters)
        {
            return response.Template.Service.Process(response, parameters, _errors);
        }
    }
}
