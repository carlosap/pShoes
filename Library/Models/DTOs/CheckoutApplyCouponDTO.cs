using Newtonsoft.Json;

namespace Library.Models
{
    public class CheckoutApplyCouponDTO
    {
        [JsonProperty("status")]                        public string Status                                    { get; set; }
        [JsonProperty("message")]                       public string Message                                   { get; set; }
        [JsonProperty("success")]                       public bool Success                                     { get; set; }
    }
}
