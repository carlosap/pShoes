using System;
namespace Library.Models.PowerReview
{
    public class Reviews
    {
        public Datum[] data { get; set; }
    }
    public class Datum
    {
        public bool is_syndicated { get; set; }
        public string comments { get; set; }
        public string reviewer_type { get; set; }
        public int rating { get; set; }
        public DateTime last_modified_date { get; set; }
        public int merchant_id { get; set; }
        public string locale { get; set; }
        public DateTime created_datetime { get; set; }
        public string page_id { get; set; }
        public int locale_id { get; set; }
        public int helpful_score { get; set; }
        public string shared_review_id { get; set; }
        public Review_Extras[] review_extras { get; set; }
        public int merchant_group_id { get; set; }
        public int profile_id { get; set; }
        public string variant { get; set; }
        public string name { get; set; }
        public int provider_id { get; set; }
        public string location { get; set; }
        public string created_date { get; set; }
        public string headline { get; set; }
        public Msqcs_And_Tags[] msqcs_and_tags { get; set; }
        public string bottomline { get; set; }
        public string merchant_user_id { get; set; }
        public int helpful { get; set; }
        public int not_helpful { get; set; }
    }

}
