using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;
using Enums;
using Library.Cache;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "Init", RequestType = typeof(InitRequest),
        ResponseType = typeof(Response<InitResponse>))]
    public class Init : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        private readonly Response<InitResponse> _result = new Response<InitResponse>();
        private InitRequest _request;

        public Init()
        {
            _errors = new List<SiteError>();
        }

        public Init(ICore core) : this()
        {
            _core = core;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            try
            {
                if (null == parameters)
                {
                    parameters = new InitRequest();
                }

                RequestHeaderHelper.GetClientIP(_core);
                _request = (InitRequest) parameters;
                GetDashByDate();
                GetCartDetails();
                GetCheckout();
                SetMenuAndBrands();
                SetSessionInfo();
                SetPingdomStatus();
                //SetLatLonPosition();
                SetEnvironment();
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("({0}){1}", ex.Message, ex.StackTrace);
                _result.errors.Add(ex.Handle("Init: " + errorMsg, ErrorSeverity.FollowUp, ErrorType.RequestError));

            }
            //FIX ME:CPEREZ. Make me a stand along service to be call from front-end
            _result.resultset.GeoLat = 0;
            _result.resultset.GeoLon = 0;
            return _result;
        }

        private void SetPingdomStatus()
        {
            try
            {
                if (_result.resultset != null)
                {
                    if (_result.resultset.Menu == null || _result.resultset.Menu.Count < 1)
                    {     
                        //cperez: pingdomsucess still cover as we check for "AllBrandsBreakdown"
                        //Code below was added to give a retry to FTP file,Menu and Brands to rebuild and
                        //prevent unclickable front-end behaviors.
                        _result.errors.Clear();
                        CacheMemory.ClearCmsCache();                
                        SetMenuAndBrands();
                    }

                    if (!_result.success
                     || _result.resultset.ContentRows == null
                     || _result.resultset.ContentRows.Count < 1
                     || _result.resultset.AllBrandsBreakdown == null
                     || _result.resultset.AllBrandsBreakdown.Count < 1
                     || _result.errors.Count > 0)
                    {
                        _result.resultset.PingdomSuccess = false;
                    }                 
                }
            }
            catch (Exception ex)
            {
                _result.errors.Add(ex.Handle("Init.SetPingdomStatus: " + ex, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private void SetEnvironment()
        {
            try
            {
                if (_result != null && _result.resultset != null) 
                {
                    #if (PRODUCTION)
                        _result.resultset.Environment = "PRODUCTION";
                    #elif (CS1)
                        _result.resultset.Environment = "CS1";      
                    #elif (STAGING)
                        _result.resultset.Environment = "STAGING";
                    #elif (UAT)
                        _result.resultset.Environment = "UAT";
                    #elif (DEV)
                        _result.resultset.Environment = "DEV";
                    #elif (DEBUG)
                        _result.resultset.Environment = "LOCALHOST";
                    #else
                        _result.resultset.Environment = "LOCALHOST";  
                    #endif 
                }
            }
            catch (Exception ex)
            {
                _result.errors.Add(ex.Handle("Init.SetEnvironment: " + ex, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private void SetSessionInfo()
        {
            try
            {
                _result.resultset.ClientIP = RequestHeaderHelper.GetClientIP(_core);
                _result.resultset.ID = EnvironmentHelper.GetLastIPAddressOctet();
                if (_core.Context.Session != null) _result.resultset.SessionID = _core.Context.Session.SessionID;
            }
            catch (Exception ex)
            {
                _result.errors.Add(ex.Handle("Init.SetSessionInfo: " + ex, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private void SetMenuAndBrands()
        {
            try
            {
                var menu = new Menu(_core, _errors);
                var menuRequest = new MenuRequest();
                var menuResponse = menu.Execute(menuRequest);
                _result.resultset.Menu = ((Response<MenuResponse>) menuResponse).resultset.Menu;
                _result.resultset.AllBrandsBreakdown = FormatBrands(_result.resultset.Menu.Where(a => a.Name == "Brands").ToList());
            }
            catch (Exception ex)
            {
                _result.errors.Add(ex.Handle("Init.SetMenuAndBrands: " + ex, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }

        private void SetLatLonPosition()
        {
            try
            {

                var getLanLonPosition = new GetLatLonPosition(_core, _errors);
                var getLanLonPositionResponse = getLanLonPosition.Execute(new EmptyRequest());
                if (getLanLonPositionResponse == null) return;
                var positionResponse = ((Response<GetLanLonPositionResponse>)getLanLonPositionResponse).resultset;
                if (positionResponse == null) return;
                _result.resultset.GeoLat = positionResponse.Latitude;
                var lonPositionResponse = positionResponse;
                {
                    var lanLonPositionResponse = lonPositionResponse;
                    _result.resultset.GeoLon = lanLonPositionResponse.Longitude;
                }
            }
            catch (Exception ex)
            {
                _result.errors.Add(ex.Handle("Init.SetLatLonPosition: " + ex, ErrorSeverity.Log, ErrorType.RequestError));
            }
        }

        private void GetDashByDate()
        {
            try
            {
                var madDash = new PaylessMadCms(_core);
                var forwardDate = GetDateFromRequest();
                _result.resultset = madDash.GetInitData(forwardDate);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void GetCheckout()
        {
            try
            {
                var session = new PaylessSession(_core);
                var checkout = session.GetCheckout();
                if (checkout == null) return;
                _result.resultset.IsLoggedIn = checkout.IsLoggedIn;
                _result.resultset.CartItemCount = checkout.Cart.CartItemCount;
                _result.resultset.CartTotalAmount = checkout.Cart.Summary.Total.Value;
                _result.resultset.CriteoCustomerId = checkout.Cart.CriteoCustomerId;
                _result.resultset.PromoHeader = checkout.Cart.PromoHeader;
            }
            catch (Exception ex)
            {
                _result.errors.Add(ex.Handle("Init.GetCheckout: " + ex, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }
        }


        private void GetCartDetails()
        {
            try
            {
                var cartDetail = new CartDetail(_core, _errors);
                var cartDetailRequest = new CartDetailRequest
                {
                    SourceCode = _request.SourceCode,
                    CampaignId = _request.CampaignId
                };
                cartDetail.Execute(cartDetailRequest);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private DateTime GetDateFromRequest()
        {
            var result = DateTime.Now;

            try
            {
                if (!string.IsNullOrEmpty(_request.d) && _request.d.Contains("/"))
                {
                    try
                    {
                        result = DateTime.Parse(HttpUtility.UrlDecode(_request.d));
                    }
                    catch (Exception)
                    {
                        //temp
                        result = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "failed to parse request date: [" + _request.d + "]" + ex;
                _result.errors.Add(ex.Handle("Init.GetDateFromRequest: " + msg, ErrorSeverity.FollowUp, ErrorType.RequestError));
            }


            return result;
        }

        private List<AllBrands> FormatBrands(List<MenuItem> brands)
        {
            var response = new List<AllBrands>();
            foreach (var sub in brands)
            {
                var result = new AllBrands {Brands = new List<MenuItem>()};
                foreach (var sub2 in sub.Subs)
                {
                    sub2.Subs = sub2.Subs;
                    result.GroupName = "A through D";
                    foreach (var innersub in sub2.Subs)
                    {
                        if (innersub.Name.StartsWith("a", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("b", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("c", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("d", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.Brands.Add(new MenuItem {Href = innersub.Href, Name = innersub.Name});
                        }
                    }
                }
                result.Brands = result.Brands.OrderBy(a => a.Name).ToList();
                response.Add(result);
            }

            foreach (var sub in brands)
            {
                var result = new AllBrands {Brands = new List<MenuItem>()};
                foreach (var sub2 in sub.Subs)
                {
                    sub2.Subs = sub2.Subs;
                    result.GroupName = "E through P";
                    foreach (var innersub in sub2.Subs)
                    {
                        if (innersub.Name.StartsWith("e", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("f", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("g", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("h", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("i", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("j", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("k", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("l", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("m", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("n", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("o", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("p", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.Brands.Add(new MenuItem {Href = innersub.Href, Name = innersub.Name});
                        }
                    }
                }
                result.Brands = result.Brands.OrderBy(a => a.Name).ToList();
                response.Add(result);
            }
            foreach (var sub in brands)
            {
                var result = new AllBrands {Brands = new List<MenuItem>()};
                foreach (var sub2 in sub.Subs)
                {
                    sub2.Subs = sub2.Subs;
                    result.GroupName = "Q through Z";
                    foreach (var innersub in sub2.Subs)
                    {
                        if (innersub.Name.StartsWith("q", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("r", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("s", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("t", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("u", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("v", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("w", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("x", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("y", StringComparison.InvariantCultureIgnoreCase) ||
                            innersub.Name.StartsWith("z", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.Brands.Add(new MenuItem {Href = innersub.Href, Name = innersub.Name});
                        }
                    }
                }
                result.Brands = result.Brands.OrderBy(a => a.Name).ToList();
                response.Add(result);
            }
            return response;
        }
    }
}