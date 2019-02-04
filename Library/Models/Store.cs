using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Library.DemandWare.Models.DTOs;
using Library.Helpers;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class Store : LocationBase
    {
        public new int Id { get; set; }
        public List<StoreHours> Hours { get; set; }
        public string StoreHourMsg { get; set; }
        public Store()
        {
            Hours = new List<StoreHours>();
        }

        public Store(DWStore apiStore) : this()
        {
            Id = apiStore.Id;
            string[] dayOfWeekArray = { "Sun.", "Mon.", "Tue.", "Wed.", "Thu.", "Fri.", "Sat." };
            Address.Address1 = (apiStore.Address1 ?? string.Empty).Trim();
            Address.Address2 = (apiStore.Address2 ?? string.Empty).Trim();
            Address.City = apiStore.City;
            Address.State = apiStore.State;
            Address.Zip = apiStore.Zip;
            Address.Phone = apiStore.Phone;
            Latitude = (double)apiStore.Latitude;
            Longitude = (double)apiStore.Longitude;
            Distance = (double) Math.Round(apiStore.Distance, 2);
            if (apiStore.Hours != null)
            {
                apiStore.Hours = apiStore.Hours.ToUpper();
                var matches = APIHelper.HoursRegex.Matches(apiStore.Hours);
                if (matches.Count > 0)
                {
                    for (int i = 0; i < matches.Count; i++)
                    {
                        var storehours = GetHoursAndMsg(matches[i]);
                        Hours.Add(new StoreHours(storehours.Item1, storehours.Item2, dayOfWeekArray[i]));
                    }
                }
            }
        }

        private Tuple<string, string> GetHoursAndMsg(Match matchItem)
        {
            var storeHours = matchItem.Groups[1].Value.Replace(" ", ""); //store hours
            var storeMsg = matchItem.Groups[matchItem.Groups.Count - 1].Value.Trim(); //store msg
            storeHours = APIHelper.MinutesRegex.Replace(storeHours, APIHelper.MinutesRegexReplaceString); //store hours and min
            if (!string.IsNullOrWhiteSpace(storeMsg))
            {
                if (storeHours.Length > 0)
                {
                    //ensure to meet military standard
                    if (!storeHours[0].Equals('1') && !storeHours[0].Equals('0'))
                    {
                        storeHours = storeHours.Insert(0, "0");
                    }
                }
            }
            //reformat content to match desktop
            storeHours = ReformatStoreHours(storeHours);
            return new Tuple<string, string>(storeHours, storeMsg);
        }
        private string ReformatStoreHours(string storehours)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(storehours))
                {
                    var closeTime = string.Empty;
                    string[] times = storehours.Split(new[] { "-" }, StringSplitOptions.None);
                    var openTime = string.Format("{0}:{1}", times[0].Substring(0, 2), times[0].Substring(3, 2));
                    closeTime = times[1].Length == 4 ? string.Format("{0}:{1} PM", int.Parse(times[1].Substring(0, 1)), times[1].Substring(2, 2)) : string.Format("{0}:{1} PM", int.Parse(times[1].Substring(0, 2)), times[1].Substring(3, 2));
                    var hour = int.Parse(openTime.Substring(0, 2));
                    openTime = string.Format(hour > 11 ? "{0} PM" : "{0} AM", openTime);
                    result = string.Format("{0} - {1}", MilitaryToNormalTime(openTime), MilitaryToNormalTime(closeTime)); 
                }
          
            }
            catch (Exception ex)
            {
                //avoid breaking front-end
                result = string.Empty;
            }
            return result;
        }

        private string MilitaryToNormalTime(string strMilitaryTime)
        {
            string result = string.Empty;
            string originalTime = strMilitaryTime;
            strMilitaryTime = strMilitaryTime.Replace(" PM", "").Replace(" AM", "").Trim();
            switch (strMilitaryTime)
            {
                case "24:00":
                case "2400":
                case "00:00":
                case "0000":
                    result = "12:00 AM";
                    break;

                case "01:00":
                case "0100":
                    result = "1:00 AM";
                    break;

                case "02:00":
                case "0200":
                    result = "2:00 AM";
                    break;

                case "03:00":
                case "0300":
                    result = "3:00 AM";
                    break;

                case "04:00":
                case "0400":
                    result = "4:00 AM";
                    break;

                case "05:00":
                case "0500":
                    result = "5:00 AM";
                    break;

                case "06:00":
                case "0600":
                    result = "6:00 AM";
                    break;

                case "07:00":
                case "0700":
                    result = "7:00 AM";
                    break;

                case "08:00":
                case "0800":
                    result = "8:00 AM";
                    break;

                case "09:00":
                case "0900":
                    result = "9:00 AM";
                    break;

                case "10:00":
                case "1000":
                    result = "10:00 AM";
                    break;

                case "11:00":
                case "1100":
                    result = "11:00 AM";
                    break;

                case "12:00":
                case "1200":
                    result = "12:00 PM";
                    break;


                case "13:00":
                case "1300":
                    result = "1:00 PM";
                    break;

                case "14:00":
                case "1400":
                    result = "2:00 PM";
                    break;

                case "15:00":
                case "1500":
                    result = "3:00 PM";
                    break;

                case "16:00":
                case "1600":
                    result = "4:00 PM";
                    break;

                case "17:00":
                case "1700":
                    result = "5:00 PM";
                    break;

                case "18:00":
                case "1800":
                    result = "6:00 PM";
                    break;

                case "19:00":
                case "1900":
                    result = "7:00 PM";
                    break;

                case "20:00":
                case "2000":
                    result = "8:00 PM";
                    break;

                case "21:00":
                case "2100":
                    result = "9:00 PM";
                    break;

                case "22:00":
                case "2200":
                    result = "10:00 PM";
                    break;

                case "23:00":
                case "2300":
                    result = "11:00 PM";
                    break;

                default:
                    result = originalTime;
                    break;
            }
            return result;
        }
    }

    public class StoreHours
    {
        public StoreHours(string hours, string message, string dayofweek)
        {
            Hours = hours;
            Message = message;
            DayOfWeek = dayofweek;
        }
        public string Hours { get; set; }
        public string Message { get; set; }
        public string DayOfWeek { get; set; }

    }
}
