using Enums;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using MadServ.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "GetLatLonPosition", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<GetLanLonPositionResponse>))]
    public class GetLatLonPosition : IRequest
    {
        #region constructor and parameters
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public GetLatLonPosition(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }
        public GetLatLonPosition(ICore core)
        {
            _core = core;
            _errors = new List<SiteError>();
        }
        public GetLatLonPosition()
        {
            _errors = new List<SiteError>();
        }
        #endregion

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var communicationRequest = BuildUrl(parameters);
            _response = Communicate(communicationRequest);
            var result = ParseResponse(_response, parameters);
            return result;
        }

        private ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            try
            {
                string url = Config.Urls.SecureBaseUrl + Config.Urls.Stores;       

                _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("GetLatLonPosition.BuildUrl", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return _core.CommunicationRequest;
        }

        public IResultResponse Communicate(ICommunicationRequest request)
        {
            try
            {
                var userIP = GetClientIPAddress(_core.Context.Request);
                if (!string.IsNullOrEmpty(userIP))
                {
                    request.AddHeader("X-FORWARDED-FOR", userIP);
                }

                request.OptionalRemoveScriptTags = false;
                var resultResponse = _core.RequestManager.Communicate(request);
                return resultResponse;
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("GetLatLonPosition.Communicate", ErrorSeverity.FollowUp, ErrorType.BuildUrl));
            }

            return new ResultResponse();
        }

        private IResponseBase ParseResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<GetLanLonPositionResponse>();

            var ns = response.XDocument.Root.GetDefaultNamespace();

            var scriptTag = response.XDocument.Descendants(ns + "script").Where(script => 
            {
                return script.Value.Contains("geolat") && script.Value.Contains("geolong");
            }).FirstOrDefault();

            if (scriptTag != null)
            {
                var script = scriptTag.Value;
                var latLine = GetSubString(script, "var geolat=", ";");
                var lonLine = GetSubString(script, "var geolong=", ";");

                var lat = GetSubString(latLine, "'", "'").Replace("'", "");
                var lon = GetSubString(lonLine, "'", "'").Replace("'", "");

                if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                {
                    result.resultset.Latitude = double.Parse(lat);
                    result.resultset.Longitude = double.Parse(lon);
                }
            }
            return result;
        }

        private string GetSubString(string input, string start, string end)
        {
            Regex regex = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            Match match = regex.Match(input);
            if (match.Success) 
            {
                return match.Value;
            }
            return "";
        }

        private string GetClientIPAddress(HttpRequestBase httpRequest)
        {
            var result = string.Empty;
            try
            {
                result = httpRequest.UserHostAddress;
                if (httpRequest.ServerVariables != null)
                {
                    var forwardedFor = httpRequest.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (!string.IsNullOrEmpty(forwardedFor))
                    {
                        var splitList = forwardedFor.Replace(" ", "").Split(',');
                        foreach (var ip in splitList)
                        {
                            var ipWithoutPort = ip.Split(':').ToList().FirstOrDefault();
                            result = ipWithoutPort;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("GetLatLonPosition.GetClientIPAddress", ErrorSeverity.FollowUp, ErrorType.Parsing));
            }
            return result;
        }
    }
}
