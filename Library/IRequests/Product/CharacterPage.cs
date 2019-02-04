using Enums;
using Library.Models;
using Library.Models.Requests;
using Library.Models.Responses;
using Library.RequestHandler;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Library.Cache;

namespace Library.IRequests
{
    [Export(typeof(IRequest))]
    [RequestAttributes(ClientId = Config.ClientId, ActionRequest = "CharacterPage", RequestType = typeof(EmptyRequest), ResponseType = typeof(Response<CharacterPageResponse>))]
    public class CharacterPage : IRequest
    {
        public ICore _core { get; set; }
        public List<SiteError> _errors { get; set; }
        public IResultResponse _response { get; set; }
        public CharacterPage()
        {
            _errors = new List<SiteError>();
        }
        public CharacterPage(ICore core)
            : this()
        {
            _core = core;
        }

        public IResponseBase Execute(IRequestParameter parameters)
        {
            var result = new Response<CharacterPageResponse>();

            try
            {
                var communicationRequest = BuildUrl(parameters);
                _response = Communicate(communicationRequest);
                result = ParseResponse(_response, parameters);
                result.template = "character";
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle("CharacterPage.Execute", ErrorSeverity.FollowUp, ErrorType.RequestError));
            }

            return result;
        }

        public ICommunicationRequest BuildUrl(IRequestParameter parameters)
        {
            string url = Config.Urls.BaseUrl + Config.Urls.CharacterPageUrl;

            _core.CommunicationRequest = new ExtendedComRequest(HttpRequestMethod.GET, url, _core, _errors);

            return _core.CommunicationRequest;
        }

        public IResultResponse Communicate(ICommunicationRequest request)
        {
            return _core.RequestManager.Communicate(request);
        }

        public Response<CharacterPageResponse> ParseResponse(IResultResponse response, IRequestParameter parameters)
        {
            var result = new Response<CharacterPageResponse>();

            XDocument xDoc = response.XDocument;
            XNamespace ns = xDoc.Root.GetDefaultNamespace();

            var container = xDoc.Descendants(ns + "div")
                                .WhereAttributeEquals("id", "characterLandingPage");

            result.resultset.Items = container.Descendants(ns + "div")
                                                .WhereAttributeContains("class", "productContainers")
                                                .Select(div =>
                                                    {
                                                        // get all classes assigned to this div
                                                        var classNames = div.AttributeValue("class");

                                                        // remove all other classes leaving just the size
                                                        var size = classNames.Replace("productContainers", "")
                                                                            .Replace("show", "")
                                                                            .Replace("hide", "")
                                                                            .Trim();

                                                        var imgEl = div.Descendants(ns + "img")
                                                                        .FirstOrNewXElement();

                                                        var imgSrc = imgEl.AttributeValue("src");
                                                      
                                                        var url = div.ElementsBeforeSelf(ns + "a")
                                                                    .LastOrDefault()
                                                                    .AttributeValue("href")
                                                                    .Replace(Config.Urls.BaseUrl, "");

                                                        var desc = div.Descendants(ns + "div")
                                                                    .WhereAttributeEquals("class", "overlayCTA")
                                                                    .FirstOrNewXElement()
                                                                    .ElementValue()
                                                                    .Trim();

                                                        return new LandingPageItem()
                                                        {
                                                            isHidden = !classNames.Contains("show"),
                                                            Description = desc,
                                                            Size = size,
                                                            PageDetailUrl = url,
                                                            Image = new Image()
                                                            {
                                                                Src = imgSrc,
                                                                Description = desc
                                                            },
                                                            Links = new List<KeyValuePair<string, string>>() 
                                                            {
                                                                new KeyValuePair<string,string>(desc, url)
                                                            }
                                                        };
                                                    })
                                                .ToList();

            result.resultset.Template = "CharacterPage_grid";

            return result;
        }
    }
}
