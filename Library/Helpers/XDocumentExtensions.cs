using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MadServ.Core.Extensions;

namespace Library.Extensions
{
    public static class XDocumentExtensions
    {
        public static IEnumerable<XElement> Divs(this XElement element)
        {
            return element.Descendants("div");
        }

        public static IEnumerable<XElement> Divs(this XDocument xDoc)
        {
            return xDoc.Descendants("div");
        }

        public static XElement FirstDescendant(this XElement element, string elementName)
        {
            return element.Descendants(elementName).FirstOrNewXElement();
        }

        public static XElement FirstDescendant(this XDocument document, string elementName)
        {
            return document.Descendants(elementName).FirstOrNewXElement();
        }

        public static XElement FirstDescendantByClass(this XElement element, string elementName, string @class)
        {
            return element.Descendants(elementName).FirstByClass(@class);
        }

        public static XElement FirstDescendantByClass(this XDocument document, string elementName, string @class)
        {
            return document.Descendants(elementName).FirstByClass(@class);
        }

        public static string FirstDescendantValue(this XElement element, string elementName)
        {
            return element.FirstDescendant(elementName).Value.Trim();
        }

        public static string FirstDescendantValue(this XDocument document, string elementName)
        {
            return document.FirstDescendant(elementName).Value.Trim();
        }

        public static XElement FirstDivByID(this XElement element, string id)
        {
            return element.Divs().FirstByID(id);
        }

        public static XElement FirstDivByID(this XDocument document, string id)
        {
            return document.Divs().FirstByID(id);
        }

        public static XElement FirstDivByClass(this XElement element, string @class)
        {
            return element.Divs().FirstByClass(@class);
        }

        public static XElement FirstDivByClass(this XDocument document, string @class)
        {
            return document.Divs().FirstByClass(@class);
        }

        public static XElement FirstDivByExactClass(this XElement element, string @class)
        {
            return element.Divs().FirstByExactClass(@class);
        }

        public static XElement FirstDivByExactClass(this XDocument document, string @class)
        {
            return document.Divs().FirstByExactClass(@class);
        }

        public static XElement DivByClass(this XElement element, string @class, int skip)
        {
            return element.DivsByClass(@class).Skip(skip).FirstOrNewXElement();
        }

        public static XElement DivByClass(this XDocument document, string @class, int skip)
        {
            return document.DivsByClass(@class).Skip(skip).FirstOrNewXElement();
        }

        public static IEnumerable<XElement> DivsByClass(this XElement element, string @class)
        {
            return element.Divs().ByClass(@class);
        }

        public static IEnumerable<XElement> DivsByClass(this XDocument document, string @class)
        {
            return document.Divs().ByClass(@class);
        }

        public static IEnumerable<XElement> DivsByExactClass(this XElement element, string @class)
        {
            return element.Divs().ByExactClass(@class);
        }

        public static IEnumerable<XElement> DivsByExactClass(this XDocument document, string @class)
        {
            return document.Divs().ByExactClass(@class);
        }

        public static IEnumerable<XElement> ByID(this IEnumerable<XElement> elements, string id)
        {
            return elements.WhereAttributeEquals("id", id);
        }

        public static IEnumerable<XElement> ByClass(this IEnumerable<XElement> elements, string @class)
        {
            return elements.WhereAttributeContains("class", @class);
        }

        public static IEnumerable<XElement> ByExactClass(this IEnumerable<XElement> elements, string @class)
        {
            return elements.WhereAttributeEquals("class", @class);
        }

        public static XElement FirstByID(this IEnumerable<XElement> elements, string id)
        {
            return elements.ByID(id).FirstOrNewXElement();
        }

        public static XElement FirstByClass(this IEnumerable<XElement> elements, string @class)
        {
            return elements.ByClass(@class).FirstOrNewXElement();
        }

        public static XElement FirstByExactClass(this IEnumerable<XElement> elements, string @class)
        {
            return elements.ByExactClass(@class).FirstOrNewXElement();
        }
    }
}
