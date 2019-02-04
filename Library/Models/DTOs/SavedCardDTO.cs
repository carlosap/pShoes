using Newtonsoft.Json;

namespace Library.Models
{
    public class SavedCardDTO
    {
        [JsonProperty("maskedNumber")]                  public string MaskedNumber                              { get; set; }
        [JsonProperty("holder")]                        public string Holder                                    { get; set; }
        [JsonProperty("type")]                          public string Type                                      { get; set; }
        [JsonProperty("expirationMonth")]               public string ExpirationMonth                           { get; set; }
        [JsonProperty("expirationYear")]                public string ExpirationYear                            { get; set; }
        [JsonProperty("isSubscription")]                public bool IsSubscription                              { get; set; }
        [JsonProperty("maskedFourDigit")]               public string MaskedFourDigit                           { get; set; }
    }
}
