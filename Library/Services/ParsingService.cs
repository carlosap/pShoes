using Enums;
using MadServ.Core.Extensions;
using MadServ.Core.Helpers;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Library.Services
{
    public class ParsingService : IService
    {
        protected readonly ICore _core;
        public List<SiteError> _errors { get; set; }
        protected XDocument _xDoc;
        protected XNamespace _ns;

        public ParsingService(ICore core)
        {
            _core = core;
        }

        public IResponseBase Process(IResultResponse xResponse, IRequestParameter parameters, List<SiteError> errors)
        {
            _errors = errors;
            _xDoc = xResponse.XDocument;
            if (_xDoc != null)
            {
                _ns = _xDoc.Root.GetDefaultNamespace();
            }

            var result = xResponse.Template.Method(xResponse, parameters);

            _errors.AddRange(ParseGeneralErrors());

            return result;
        }

        protected List<SiteError> ParseGeneralErrors()
        {
            List<SiteError> result = new List<SiteError>();

            try
            {
                result = _xDoc.Descendants(_ns + "div")
                              .Where(z => z.AttributeValue("class") == "error" || z.AttributeValue("class") == "error-form")
                              .Select(x => new SiteError
                              {
                                  Message = new ErrorMessage(x.ElementValue(), string.Empty),
                                  Severity = ErrorSeverity.UserActionRequired,
                                  Type = ErrorType.UserActionRequired
                              })
                              .ToList();

                result.AddRange(_xDoc.Descendants(_ns + "span")
                                     .Where(z => z.AttributeValue("class").Contains("error"))
                                     .Select(x => new SiteError
                                     {
                                         Message = new ErrorMessage(x.ElementValue(), string.Empty),
                                         Severity = ErrorSeverity.UserActionRequired,
                                         Type = ErrorType.UserActionRequired
                                     })
                                     .ToList());
            }
            catch (Exception ex)
            {
                _errors.Add(ex.Handle(
                "MadServ.AccountService.ParseGeneralErrors",
                ErrorSeverity.FollowUp,
                ErrorType.Parsing
                ));
            }

            return result;
        }

        protected XElement GetElementByClass(string elementName, string className, XElement xElement = null)
        {
            if (xElement != null)
            {
                return xElement.Descendants(_ns + elementName).WhereAttributeEquals("class", className).FirstOrNewXElement();
            }
            else
            {
                return _xDoc.Descendants(_ns + elementName).WhereAttributeEquals("class", className).FirstOrNewXElement();
            }
        }

        protected XElement GetElementByID(string elementName, string ID, XElement xElement = null)
        {
            if (xElement != null)
            {
                return xElement.Descendants(_ns + elementName).WhereAttributeEquals("id", ID).FirstOrNewXElement();
            }
            else
            {
                return _xDoc.Descendants(_ns + elementName).WhereAttributeEquals("id", ID).FirstOrNewXElement();
            }
        }

        protected XElement GetDivByClass(string className, XElement xElement = null)
        {
            return GetElementByClass("div", className, xElement);
        }

        protected XElement GetDivByID(string ID, XElement xElement = null)
        {
            return GetElementByID("div", ID, xElement);
        }

        protected XElement GetTDByClass(string className, XElement xElement = null)
        {
            return GetElementByClass("td", className, xElement);
        }

        protected XElement GetTDByID(string ID, XElement xElement = null)
        {
            return GetElementByClass("td", ID, xElement);
        }
    }
}
