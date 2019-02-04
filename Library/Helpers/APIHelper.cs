using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using Library.eCRM;
using MadServ.Core.Models;

namespace Library.Helpers
{
    public static class APIHelper
    {
        public static Regex HoursRegex = new Regex(@":([0-9\s-,]*(MIDNIGHT)?)?([\w\s]*)", RegexOptions.ECMAScript);
        public static Regex MinutesRegex = new Regex(@"(?<min>[\d]{2}$)|(?<min>[\d]{2}-)", RegexOptions.Compiled);
        public static string MinutesRegexReplaceString = ":${min}";
        public static string GetOptimizedImageSrc(string src, int width = Config.Params.ImageWidth)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(src)) return result;
            var begin = src.IndexOf("/on/demandware.static");
            result = begin > -1 ? string.Format("{0}{1}?sw={2}", Config.Urls.DWImageOptimizerUrl, src.Substring(begin, src.Length - begin), width) : src;
            return result;
        }
        public static List<Option> GetCardMonths()
        {
            var months = new List<Option>();
            for (var i = 0; i < 12; i++)
                months.Add(new Option()
                {
                    Name = CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames[i],
                    Value = (i + 1).ToString(),
                    IsSelected = DateTime.Now.Month.Equals(i + 1)
                });

            return months;
        }

        public static List<Option> GetCardYears()
        {
            var years = new List<Option>();
            for (var i = 0; i < 8; i++)
            {
                var year = DateTime.Now.Year + i;
                years.Add(new Option()
                {
                    Name = year.ToString(),
                    Value = year.ToString(),
                    IsSelected = year.Equals(DateTime.Now.Year)
                });
            }
            return years;
        }

        public static CustomBinding GetECRMbinding()
        {
            var basicHttpsBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.TransportWithMessageCredential);
            basicHttpsBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            var customBinding = new CustomBinding(basicHttpsBinding);
            var customBindingElements = customBinding.Elements;
            ((SecurityBindingElement)customBindingElements[0]).EnableUnsecuredResponse = true;
            return customBinding;
        }

        public static EnterpriseCRMServiceClient GetECRMclient(bool useProductionAPI = false)
        {
            var eCRMclient = new EnterpriseCRMServiceClient(Config.ECRMbinding, useProductionAPI ? Config.ECRMendpoint : Config.ECRMstagingEndpoint);
            eCRMclient.ClientCredentials.UserName.UserName = (useProductionAPI ? Config.Params.ECRMuserName : Config.Params.ECRMstagingUserName);
            eCRMclient.ClientCredentials.UserName.Password = (useProductionAPI ? Config.Params.ECRMpassword : Config.Params.ECRMstagingPassword);
            return eCRMclient;
        }

        public static T DeepClone<T>(T obj)
        {
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    return (T)formatter.Deserialize(ms);
                }
            }
            catch
            {
                return Activator.CreateInstance<T>();
            }
        }
    }
}
