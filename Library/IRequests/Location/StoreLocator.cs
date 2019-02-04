using Enums;
using Library.DemandWare.Models;
using Library.DemandWare.Models.DTOs;
using Library.DemandWare.RequestManager;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.IRequests;
using MadServ.Core.Models;
using MadServ.Core.Models.Requests;
using MadServ.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using Library.Models;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "StoreLocator", RequestType = typeof(StoreLocatorRequest), ResponseType = typeof(Response<StoreLocatorResponse>))]
    public class StoreLocator : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private StoreLocatorRequest _request;
        public string Zip { get; set; }
        public List<StoreByZip> StoresByZip { get; set; }
        public string TargetUrl { get; set; }

        public StoreLocator()
        {
            _errors = new List<SiteError>();
        }
        public StoreLocator(ICore core)
            : this()
        {
            _core = core;
            StoresByZip = new List<StoreByZip>();

        }
        public StoreLocator(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
            StoresByZip = new List<StoreByZip>();

        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            StoresByZip = new List<StoreByZip>();
            IResponseBase result = new Response<StoreLocatorResponse>();

            try
            {
                _request = (StoreLocatorRequest)parameters;
                Zip = _request.Zip;
                var geoLocationRequestBase = new GeoLocationRequestBase { Zip = _request.Zip, City = _request.City, State = _request.State };
                var geoLocationsResponse = ((Response<GeoLocationResponseBase>)new GeoLocation().Execute(geoLocationRequestBase));
                var geoLocation = geoLocationsResponse.resultset.Locations.FirstOrDefault();

                if (geoLocation != null)
                {
                    _request.Latitude = geoLocation.Latitude;
                    _request.Longitude = geoLocation.Longitude;
                    _request.Zip = string.Empty;
                }

                var config = BuildApiConfig(parameters);
                result = GetResponse(config);
            }
            catch(Exception ex)
            {
                _errors.Add(ex.Handle("StoreLocator.GetResponse", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            
            result = ZipLocations(result);
            return result;
        }
 
        private IResponseBase ZipLocations(IResponseBase result)
        {
                Response<StoreLocatorResponse> zipLocations = new Response<StoreLocatorResponse>();
                zipLocations = (Response<StoreLocatorResponse>)result;
                zipLocations.resultset.UrlTarget = TargetUrl;
                string dayPrefix = DateTime.Now.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                GetDayOfWeekNum(dayPrefix);
                GetZipLocationMsg(zipLocations, dayPrefix);
            return result;
        }
 
        private void GetZipLocationMsg(Response<StoreLocatorResponse> zipLocations, string dayPrefix)
        {
            //Always reindex the dayof week
            foreach (Store store in zipLocations.resultset.Locations)
            {
                try
                {
                    string storehours = store.Hours[GetDayOfWeekNum(dayPrefix)].Hours;
                    if (string.IsNullOrWhiteSpace(storehours))
                    {
                        store.StoreHourMsg = store.Hours[GetDayOfWeekNum(dayPrefix)].Message;
                        continue;
                    }
                    string startTime = storehours.Split('-').GetValue(0).ToString();
                    string endTime = storehours.Split('-').GetValue(1).ToString();
                    store.StoreHourMsg = string.Format("Open Today {0} - {1}", startTime, endTime);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private int GetDayOfWeekNum(string dayPrefix)
        {
            int dayOfWeekNum = -1;
            switch (dayPrefix)
            {
                case "SUN":
                    dayOfWeekNum = 0;
                    break;
                case "MON":
                    dayOfWeekNum = 1;
                    break;
                case "TUE":
                    dayOfWeekNum = 2;
                    break;
                case "WED":
                    dayOfWeekNum = 3;
                    break;
                case "THU":
                    dayOfWeekNum = 4;
                    break;
                case "FRI":
                    dayOfWeekNum = 5;
                    break;
                case "SAT":
                    dayOfWeekNum = 6;
                    break;
                default:
                    dayOfWeekNum = -1;
                    break;
            }
            return dayOfWeekNum;
        }

        private DWClientConfig BuildApiConfig(IRequestParameter parameters)
        {
            TargetUrl = string.Empty;
            var sb = new StringBuilder();
            sb.AppendFormat("country_code={0}", Config.Params.DefaultCountryCode);
            sb.AppendFormat("&distance_unit={0}", Config.Params.DefaultDistanceUnit);
            sb.AppendFormat("&max_distance={0}", _request.Radius);

            if (string.IsNullOrEmpty(_request.Zip))
            {
                sb.AppendFormat("&latitude={0}", _request.Latitude);
                sb.AppendFormat("&longitude={0}", _request.Longitude);
            }
            else
                sb.AppendFormat("&postal_code={0}", _request.Zip);

            return new DWClientConfig(_core)
            {
                Path = Config.DWPath.StoreLocator,
                Query = sb.ToString(),
                Method = HttpMethod.GET
            };
        }

        private IResponseBase GetResponse(DWClientConfig config)
        {
            var result = new Response<StoreLocatorResponse>();
            try
            {
                var apiResponse = DWClient.GetAPIResponse<StoreResult>(config);

                if (apiResponse.ResponseStatus.Code == HttpStatusCode.OK)
                    result.resultset = new StoreLocatorResponse(apiResponse.Model);
                else
                    _errors.Add(new SiteError
                    {
                        Message =
                            new ErrorMessage(Config.Constants.GenericError,
                                apiResponse.ResponseStatus.Message.ToString())
                    });
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("StoreLocator.GetResponse", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
            return result;
        }
    }
}
