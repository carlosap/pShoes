using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Enums;
using Library.DemandWare.Models;
using Library.DemandWare.RequestManager;
using Library.Helpers;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using Library.Cache;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "MenuRequest")]
    public class MenuRequest : IRequestParameter
    {
        public string Url { get; set; }
        public string Path { get; set; }
        public bool KeepTraversing { get; set; }
        public MenuRequest()
        {
            Url = Config.Params.HrefPrefix + "root";
            Path = string.Empty;
        }
    }
}

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "Menu", RequestType = typeof(MenuRequest),
        ResponseType = typeof(Response<MenuResponse>))]
    public class Menu : IRequest
    {

        private MenuRequest _request;
        private bool _isRoot;
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public Menu()
        {
            _errors = new List<SiteError>();
        }
        public Menu(ICore core)
            : this()
        {
            _core = core;
        }

        public Menu(ICore core, List<SiteError> errors)
        {
            _core = core;
            _errors = errors;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<MenuResponse>();
            try
            {
                _request = (MenuRequest) parameters;
                _isRoot = _request.Url.Equals(Config.Params.HrefPrefix + "root");
                result = CacheMemory.Get<Response<MenuResponse>>(Config.CacheKeys.Menu);
                if (result == null || string.IsNullOrWhiteSpace(result.template))
                {
                    if (_request.Url.StartsWith(Config.Params.HrefPrefix)) _request.Url = _request.Url.Remove(0, Config.Params.HrefPrefix.Length);
                    var config = BuildAPIConfig(parameters);
                    result = GetResponse(config);
                    result.template = Config.ResponseTemplates.Menu;
                    if (_isRoot && _errors.Count == 0)
                    {
                        CacheMemory.SetAndExpiresHoursAsync(Config.CacheKeys.Menu, result, 4);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRoot)
                {
                    _errors.Add(ex.Handle("Menu.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
                }              
            }
            return result;
        }

        private DWClientConfig BuildAPIConfig(IRequestParameter parameters)
        {
            return new DWClientConfig(_core)
            {
                Path = string.Format(Config.DWPath.Menu, _request.Url),
                Query = "levels=2",
                Method = HttpMethod.GET
            };
        }

        private Response<MenuResponse> GetResponse(DWClientConfig config)
        {
            var result = new Response<MenuResponse>();
            try
            {                 
                var dwCategoryDataCachekey = string.Format(Config.CacheKeys.DWCategoryData, config.Path);
                var apiResponse = CacheMemory.Get<APIResponse<DemandWare.Models.DTOs.Category>>(dwCategoryDataCachekey);
                if (_isRoot && result.resultset.Menu.Count == 0)
                {
                    apiResponse = null;
                }
                if (apiResponse == null || apiResponse.Model == null)
                {
                    try
                    {
                        apiResponse = DWClient.GetAPIResponse<DemandWare.Models.DTOs.Category>(config);
                        if (apiResponse != null && apiResponse.ResponseStatus.Code == HttpStatusCode.OK && apiResponse._errors == null)
                        {
                            //CacheMemory.SetAndExpiresHoursAsync(dwCategoryDataCachekey, apiResponse, 4);
                            result.resultset = new MenuResponse(apiResponse.Model, _request.Path);
                            if (_isRoot || _request.KeepTraversing)
                            {
                                SetMenuItems(result);
                                if (_isRoot)
                                {
                                    Update_MenuBy_CmsAndFtpFile(result);
                                }
                            }
                        }
                        else
                        {
                            if (apiResponse != null)
                                _errors.Add(new SiteError
                                {
                                    Message = new ErrorMessage(Config.Constants.GenericError, apiResponse.ResponseStatus.Message)
                                });
                        } 
                    }
                    catch (Exception){}
                }
            }
            catch (Exception ex)
            {
                if (_isRoot)
                {
                    _errors.Add(ex.Handle("Menu.GetResponse", ErrorSeverity.FollowUp, ErrorType.RequestError));
                }
                
            }
            return result;
        }

        private void Update_MenuBy_CmsAndFtpFile(Response<MenuResponse> result)
        {
            try
            {
                var madDash = new PaylessMadCms(_core);
                var categoryImagesLookup = madDash.CategoryImages();
                var hrefLookup = HrefLookup.Load(_core);
                if (_errors.Any() || !result.resultset.Menu.Any()) return;
                Update_MenuHrefLookup(result.resultset.Menu, categoryImagesLookup, hrefLookup);
                Add_MenuItem_GiftCard(result);
                Add_MenuItem_Help(result);
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Menu.Update_MenuBy_CmsAndFtpFile", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

        }

        private static void Add_MenuItem_Help(Response<MenuResponse> result)
        {
                result.resultset.Menu.Add(new MenuItem
                {
                    Name = "more",
                    Href = "/help",
                    Path = "more"

                });
        }

        private static void Add_MenuItem_GiftCard(Response<MenuResponse> result)
        {
            result.resultset.Menu.Add(new MenuItem
            {
                Name = "gift cards",
                Href = "/giftcards/74935.html",
                Path = "gift cards"
            });
        }

        private void SetMenuItems(Response<MenuResponse> result)
        {
            foreach (var menuItem in result.resultset.Menu)
            {
                try
                {
                    var item = menuItem;
                    if (menuItem.Subs.Count <= 0) continue;
                    foreach (var sub in menuItem.Subs)
                    {
                        var menu = new Menu(_core, _errors);
                        var menuRequest = new MenuRequest
                        {
                            Url = sub.CategoryId,
                            Path = item.Name.ToLower().Replace(" ", "-"),
                            KeepTraversing = false
                        };
                        var menuSubItems = GetSubMenu(sub, menu, menuRequest);
                        if (menuSubItems.Count > 0)
                            sub.Subs = menuSubItems;

                    }
                }
                catch (Exception e)
                {
                    //todo report to relic
                    //do not block
          
                }
            }
        }

        private static List<MenuItem> GetSubMenu(MenuItem sub, Menu menu, MenuRequest menuRequest)
        {
            List<MenuItem> menuList;
            try
            {
                menuList = ((Response<MenuResponse>) menu.Execute(menuRequest)).resultset.Menu;
            }
            catch (Exception)
            {
                return new List<MenuItem>();
            }
            return menuList;
        }

        private void Update_MenuHrefLookup(List<MenuItem> menu, NameValueCollection categoryImagesLookup, HrefLookup hrefLookup)
        {
            try
            {
                menu.ForEach(item =>
                {
                    item.Image.Src = categoryImagesLookup.Get(item.CategoryId);
                    item.Href = hrefLookup.Reverse.Get(item.CategoryId) ?? "#";
                    Update_MenuHrefLookup(item.Subs, categoryImagesLookup, hrefLookup);
                });
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("Menu.Update_MenuHrefLookup", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

        }
    }
}
