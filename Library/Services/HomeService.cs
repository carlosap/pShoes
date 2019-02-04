using Library.Models.Responses;
using MadServ.Core.Extensions;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Library.Services
{
    public class HomeService : IService
    {
        private readonly ICore _core;
        public List<SiteError> _errors { get; set; }
        private XDocument _xDoc;
        private XNamespace _ns;

        public HomeService(ICore core)
        {
            _core = core;
        }

        public IResponseBase Process(IResultResponse xResponse, IRequestParameter parameters, List<MadServ.Core.Models.SiteError> errors)
        {
            _errors = errors;
            _xDoc = xResponse.XDocument;
            if (_xDoc != null)
            {
                _ns = _xDoc.Root.GetDefaultNamespace();
            }

            return xResponse.Template.Method(xResponse, parameters);
        }

        public IResponseBase ParseFindMyPerfectShoe(IResultResponse xResponse, IRequestParameter parameters)
        {           
            var result = new Response<FindMyShoeResponse>();

            result.resultset.Items = _xDoc.Descendants(_ns + "a")
                                        .Select(a => {
                                            return new ShoeBoxMenu
                                            {
                                                Href = a.AttributeValue("href"),
                                                Name = a.Value.Trim()
                                            };
                                        }).ToList();

            return result;
        }
    }
}
