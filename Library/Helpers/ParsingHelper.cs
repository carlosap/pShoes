using Library.Cache;
using Library.Models;
using Library.Models.Responses;
using MadServ.Core.Extensions;
using MadServ.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Library.Helpers
{
    public static class ParsingHelper
    {
        public static Regex SCodeRegex = new Regex(@"(?<script>(?:s.pageName).*?)</script>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static Regex SVarRegex = new Regex(@"\s{0,1}(?<var>s\.?.*;)", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex SkuRegex = new Regex(@"/(?<sku>[\d]+).html", RegexOptions.Compiled);
        public static Regex CriteoCustomerIdRegex = new Regex("\"setCustomerId\", id: \"(?<customerId>[\\d]+)\" },", RegexOptions.Compiled);
        public static Regex BasketRegex = new Regex(@"(?<script>(?:var basket = {).*?)</script>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static Regex BasketProductIdRegex = new Regex(@"identifier: '(?<id>[\d+]*)'", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex CartGoogleWalletRegex = new Regex(@"(?<script>(?:var mwRequestJwt =).*?)</script>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static Regex BillingGoogleWalletRegex = new Regex(@"(?<script>(?:var requestJwt =).*?)</script>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static Regex ReviewGoogleWalletRegex = new Regex(@"(?<script>(?:var fwRequestJwt =).*?)</script>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static XNamespace _ns;
                
        public static GoogleWalletInfo GetCartGoogleWalletInfoFrom(IResultResponse response, Cart cart)
        {
            var result = new GoogleWalletInfo();

            try
            {
                var match = CartGoogleWalletRegex.Match(response.RawData);
                if (match.Success)
                {
                    var script = match.Groups["script"].Value;

                    result.MWRequestJwt = GetScriptVariable(script, "var mwRequestJwt = \"", "\"");
                    result.PreauthFlow = GetScriptVariable(script, "var preauthFlow = ", ";").Equals("true");
                    result.ClientId = GetScriptVariable(script, "var clientId = \"", "\"");
                    result.DisableCheckout = GetScriptVariable(script, "var disableCheckout = ", ";").Equals("true") || cart.ExceedsMaxForGoogleWallet;
                    result.TrackJwt = GetScriptVariable(script, "var trackJwt = \"", "\"");
                }
            }
            catch
            {
            }

            return result;
        }

        //cperez: googlewallet was retired whileback. to review and consider removing.
        public static GoogleWalletInfo GetBillingGoogleWalletInfo(IResultResponse response, Cart cart)
        {
            var result = new GoogleWalletInfo();

            try
            {
                var match = BillingGoogleWalletRegex.Match(response.RawData);
                if (match.Success)
                {
                    var script = match.Groups["script"].Value;

                    result.MWRequestJwt = GetScriptVariable(script, "var requestJwt = '", "'");
                    result.ClientId = GetScriptVariable(script, "\"clientId\": \"", "\"");
                    result.TrackJwt = GetScriptVariable(script, "var trackJwt = \"", "\"");

                    result.DisableCheckout = cart.ExceedsMaxForGoogleWallet;
                }
            }
            catch
            {
            }

            return result;
        }

        public static GoogleWalletInfo GetReviewGoogleWalletInfoFrom(IResultResponse response, Cart cart)
        {
            var result = new GoogleWalletInfo();

            try
            {
                var match = ReviewGoogleWalletRegex.Match(response.RawData);
                if (match.Success)
                {
                    var script = match.Groups["script"].Value;

                    result.FWRequestJwt = GetScriptVariable(script, "var fwRequestJwt = '", "'");
                    result.CWRequestJwt = GetScriptVariable(script, "var cwRequestJwt = '", "'");
                    result.TrackJwt = GetScriptVariable(script, "var trackJwt = \"", "\"");

                    result.DisableCheckout = cart.ExceedsMaxForGoogleWallet;
                }
            }
            catch
            {
            }

            return result;
        }

        public static string GetScriptVariable(string script, string start, string end)
        {
            var result = string.Empty;

            if (!string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var startIndex = script.IndexOf(start, StringComparison.InvariantCultureIgnoreCase);

                if (startIndex > -1)
                {
                    var valueStartIndex = startIndex + start.Length;
                    var endIndex = script.IndexOf(end, valueStartIndex, StringComparison.InvariantCultureIgnoreCase);

                    if (endIndex > -1)
                    {
                        result = script.Substring(valueStartIndex, endIndex - valueStartIndex);
                    }
                }
            }

            return result;
        }

        public static string GetDWQuery(XDocument xDoc, string formId)
        {
            var result = string.Empty;
            var ns = xDoc.Root.GetDefaultNamespace();

            var action = xDoc.Descendants(ns + "form")
                             .WhereAttributeEquals("id", formId)
                             .FirstOrNewXElement()
                             .AttributeValue("action");

            if (!string.IsNullOrEmpty(action))
            {
                var begin = action.IndexOf("?dwcont=");

                if (begin > -1)
                {
                    result = action.Substring(begin, action.Length - begin);
                }
            }

            return result;
        }

        public static string GetTemplateHeader(XDocument xDoc, XNamespace ns)
        {
            return xDoc.Descendants(ns + "div")
                       .WhereAttributeEquals("id", "primary")
                       .FirstOrNewXElement()
                       .Descendants(ns + "h1")
                       .FirstOrNewXElement()
                       .ElementValue();
        }






        public static string GetSkuFromHref(string href)
        {
            var result = string.Empty;
            var match = SkuRegex.Match(href);

            if (match.Success)
            {
                result = match.Groups["sku"].Value;
            }

            return result;
        }

        public static string GetCriteoCustomerId(IResultResponse response)
        {
            var result = string.Empty;
            var match = CriteoCustomerIdRegex.Match(response.RawData);

            if (match.Success)
            {
                result = match.Groups["customerId"].Value;
            }

            return result;
        }

        public static List<string> GetProductMasterIds(IResultResponse response)
        {
            var result = new List<string>();

            var match = BasketRegex.Match(response.RawData);
            if (match.Success)
            {
                var script = match.Groups["script"].Value;
                var productIdsMatches = BasketProductIdRegex.Matches(script);

                if (productIdsMatches != null)
                {
                    foreach (Match id in productIdsMatches)
                    {
                        result.Add(id.Groups["id"].Value);
                    }
                }
            }

            return result;
        }

        public static string GetHrefWithoutQueryString(string href)
        {
            var result = href;

            var questionMark = result.IndexOf("?");

            if (questionMark > 0)
            {
                result = result.Substring(0, questionMark);
            }

            var hashMark = result.IndexOf("#");

            if (hashMark > 0)
            {
                result = result.Substring(0, hashMark);
            }

            if (result.EndsWith("/"))
            {
                result = result.Substring(0, result.Length - 1);
            }

            //this is used to test CMS changes
            result = result.ToLowerInvariant().Replace("/forwarddate", "");

            return result;
        }

        public static string CustomStreamReaderProcess(StreamReader streamReader)
        {
            StringBuilder sb = new StringBuilder();
            string s = streamReader.ReadLine();
            while (s != null)
            {
                s = s.Trim();
                if (s.Length > 0)
                {
                    s = s.Replace(@"\t", "").Replace(@"\n", "").Replace(@"\r", "").Replace(@"\""", "");

                    //this was for malformed html associated with new McAfee image in cart and checkout
                    //s = s.Replace("oncontextmenu=\"alert(\"Copying Prohibited by Law - McAfee SECURE is a Trademark of McAfee, Inc.\"); return false;\"", "");

                    if (s.Length > 0)
                    {
                        sb.Append(s);
                    }
                }
                s = streamReader.ReadLine();
            }
            return sb.ToString()
                .Replace("”", "\"");
        }
        public static string GetCheckout_CsrfToken(XDocument xDoc)
        {
            //Checkout Parser
            _ns = xDoc.Root.GetDefaultNamespace();
            var csrfToken = xDoc.Descendants(_ns + "div")
                .WhereAttributeEquals("class", "ordersummary")
                .Descendants(_ns + "form")
                .FirstOrNewXElement()
                .AttributeValue("action");

            return csrfToken;
        }

        public static string GetPasswordReset_CsrfToken(XDocument xDoc)
        {
            //Cart parser
            _ns = xDoc.Root.GetDefaultNamespace();
            var csrfToken = xDoc.Descendants(_ns + "a")
                                    .WhereAttributeEquals("id", "password-reset")
                                    .FirstOrNewXElement()
                                    .AttributeValue("href");


            return csrfToken;
        }

        public static dynamic GetTealiumDataBase(XDocument xDoc)
        {
            dynamic tealiumDataBase = null;
            _ns = xDoc.Root.GetDefaultNamespace();

            var tealiumScriptTag = xDoc.Descendants(_ns + "script").Where(script =>
            {
                return script.Value.Contains("var utag_data =");
            }).FirstOrDefault();

            if (tealiumScriptTag != null)
            {
                //Remove unnecessary characters and rename the JS variable.
                var tealiumData = tealiumScriptTag.Value.Replace("//var utag_data =", "").Replace("};//", "}");
                tealiumDataBase = JsonConvert.DeserializeObject<dynamic>(tealiumData);
            }
            return tealiumDataBase;
        }

        public static dynamic GetTealiumDataExtended(XDocument xDoc)
        {
            dynamic tealiumDataExtended = null;
            _ns = xDoc.Root.GetDefaultNamespace();

            var tealiumScriptTag = xDoc.Descendants(_ns + "script").Where(script =>
            {
                return script.Value.Contains("var utag_payless_data =");
            }).FirstOrDefault();

            if (tealiumScriptTag != null)
            {
                //Remove unnecessary characters and rename the JS variable.
                var tealiumData = tealiumScriptTag.Value.Replace("//var utag_payless_data =", "").Replace("};//", "}");
                tealiumDataExtended = JsonConvert.DeserializeObject<dynamic>(tealiumData);
            }
            return tealiumDataExtended;
        }
    }
}