using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Library.Models;
using Newtonsoft.Json;
using Library.Models.PowerReview;
namespace Library.PowerReview
{
    public static class PowerReviewHelper
    {
        private static string _requestUri;
        public static List<AvgRate> GetAvgRates(string rawJson)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<AvgRate>>(rawJson);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static FaceOff GetFaceOffReviews(string rawJson)
        {
            try
            {
                var results = new FaceOff();
                dynamic dynamicObject = JsonConvert.DeserializeObject(rawJson);
                results.negative.rating = (int)dynamicObject.negative.rating.Value;
                results.negative.comments = (string)dynamicObject.negative.comments.Value;
                results.negative.headline = (string)dynamicObject.negative.headline.Value;
                results.positive.rating = (int)dynamicObject.positive.rating.Value;
                results.positive.comments = (string)dynamicObject.positive.comments.Value;
                results.positive.headline = (string)dynamicObject.positive.headline.Value;
                return results;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static List<ReviewItem> GetReviews(string rawJson)
        {
            try
            {
                var userreviews = new List<ReviewItem>();
                var reviews = JsonConvert.DeserializeObject<Reviews>(rawJson);
                return ReviewItems(reviews, userreviews);
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static List<ReviewItem> ReviewItems(Reviews reviews, List<ReviewItem> userreviews)
        {
            try
            {
                if (!reviews.data.Any()) return userreviews;
                foreach (var item in reviews.data)
                {
                    try
                    {
                        var comfortMsg = string.Empty;
                        var durabilityMsg = string.Empty;
                        var sizingMsg = string.Empty;
                        if (item.msqcs_and_tags != null)
                        {
                            foreach (var msqcsItem in item.msqcs_and_tags)
                            {
                                if (msqcsItem.tags == null) continue;
                                foreach (var tag in msqcsItem.tags)
                                {
                                    if (tag.Comfort != null)
                                    {
                                        comfortMsg = tag.Comfort.FirstOrDefault();
                                    }
                                    if (tag.Durability != null)
                                    {
                                        durabilityMsg = tag.Durability.FirstOrDefault();
                                    }
                                    if (tag.Sizing != null)
                                    {
                                        sizingMsg = tag.Sizing.FirstOrDefault();
                                    }
                                }
                            }
                        }
                        userreviews.Add(item: new ReviewItem
                        {
                            Name = item.name ?? "",
                            Rating = item.rating,
                            Headline = item.headline ?? "",
                            Bottomline = item.bottomline ?? "",
                            Location = item.location ?? "",
                            DateCreated = item.created_datetime.ToShortDateString() ?? "",
                            Comments = item.comments ?? "",
                            ComfortMsg = comfortMsg,
                            DurabilityMsg = durabilityMsg,
                            SizingMsg = sizingMsg,
                            reviewId = item.shared_review_id ?? "",
                            merchantGroupId = item.merchant_group_id.ToString() ?? "",
                            merchantId = item.merchant_id.ToString() ?? "",
                            HelpfulCount = item.helpful,
                            NotHelpfulCount = item.not_helpful,
                            TotalHelpfulVote = (item.helpful + item.not_helpful) > 0 ? (item.helpful + item.not_helpful) : 0

                        });
                    }
                    catch (Exception) {/*do nothing this code changes a lot.*/}
                }
            }
            catch (Exception ex) { userreviews = new List<ReviewItem>(); }
            return userreviews;
        }
        public static Snapshot GetSnapshot(string rawJson)
        {
            try
            {
                var snapshot = new Snapshot();
                var snapshotTemp = rawJson.Replace("\"1\":", "\"star1\":").Replace("\"2\":", "\"star2\":").Replace("\"3\":", "\"star3\":").Replace("\"4\":", "\"star4\":").Replace("\"5\":", "\"star5\":");
                snapshot = JsonConvert.DeserializeObject<Snapshot>(snapshotTemp);
                snapshot.average_rating = (Math.Round(double.Parse(snapshot.average_rating ?? "0") / 5, 1, MidpointRounding.AwayFromZero) * 5).ToString();
                return snapshot;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// cperez: 9/28/16- 
        /// Note: Msqc is basically a PowerReview "thing" - 
        /// Client is adding Custom tags and requiring business logic
        /// The hard-code values you see below are specify and were provided by
        /// client. The business logic added was to calc. the Percentage per each 
        /// Msqc Tag Category which is being attached to the resultset response. 
        /// </summary>
        /// <param name="reviews">List of user reviews</param>
        /// <returns>custom Tags - MsqcTags</returns>
        public static MsqcTags GetMsqcsTagSummary(List<ReviewItem> reviews)
        {
            try
            {
                var msqcTags = new MsqcTags();
                //----------------------Comfortable-------------------------------------------->
                var tempComfortable = reviews.Count(x => x.ComfortMsg.Equals("Comfortable"));
                var tempUncomfortable = reviews.Count(x => x.ComfortMsg.Equals("Uncomfortable"));
                var tempVeryComfortable = reviews.Count(x => x.ComfortMsg.Equals("Very Comfortable"));
                var total = (tempVeryComfortable + tempComfortable + tempUncomfortable);
                msqcTags.Comfort.TotalEntries = total;
                msqcTags.Comfort.Comfortable = GetPercentageAsString(tempComfortable, total);
                msqcTags.Comfort.Uncomfortable = GetPercentageAsString(tempUncomfortable, total);
                msqcTags.Comfort.VeryComfortable = GetPercentageAsString(tempVeryComfortable, total);
                //----------------------Durable-------------------------------------------->
                var tempDurable = reviews.Count(x => x.DurabilityMsg.Equals("Durable"));
                var tempNonDurable = reviews.Count(x => x.DurabilityMsg.Equals("Non-durable"));
                var tempVeryDurable = reviews.Count(x => x.DurabilityMsg.Equals("Very Durable"));
                total = (tempDurable + tempNonDurable + tempVeryDurable);
                msqcTags.Durability.TotalEntries = total;
                msqcTags.Durability.Durable = GetPercentageAsString(tempDurable, total);
                msqcTags.Durability.NonDurable = GetPercentageAsString(tempNonDurable, total);
                msqcTags.Durability.VeryDurable = GetPercentageAsString(tempVeryDurable, total);
                //----------------------Sizing-------------------------------------------->
                var tempFeelsTooLarge = reviews.Count(x => x.SizingMsg.Equals("Feels Too Large"));
                var tempFeelsTooSmall = reviews.Count(x => x.SizingMsg.Equals("Feels Too Small"));
                var tempFeelsTrueToSize = reviews.Count(x => x.SizingMsg.Equals("Feels True to Size"));
                total = (tempFeelsTooLarge + tempFeelsTooSmall + tempFeelsTrueToSize);
                msqcTags.Sizing.TotalEntries = total;
                msqcTags.Sizing.FeelsTooLarge = GetPercentageAsString(tempFeelsTooLarge, total);
                msqcTags.Sizing.FeelsTooSmall = GetPercentageAsString(tempFeelsTooSmall, total);
                msqcTags.Sizing.FeelsTrueToSize = GetPercentageAsString(tempFeelsTrueToSize, total);
                return msqcTags;
            }
            catch (Exception)
            {
                return new MsqcTags();
            }
        }
        private static string GetPercentageAsString(double ratio)
        {
            return string.Format("{0:0.0%}", ratio);
        }
        private static string GetPercentageAsString(int top, int bottom)
        {
            return GetPercentageAsString((double)top / bottom);
        }
    }
}