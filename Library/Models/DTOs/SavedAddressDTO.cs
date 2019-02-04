using Newtonsoft.Json;

namespace Library.Models
{
    public class SavedAddressDTO
    {
        [JsonProperty("UUID")]                          public string UUID                                      { get; set; }
        [JsonProperty("ID")]                            public string Id                                        { get; set; }
        [JsonProperty("key")]                           public string Key                                       { get; set; }
        [JsonProperty("firstName")]                     public string FirstName                                 { get; set; }
        [JsonProperty("lastName")]                      public string LastName                                  { get; set; }
        [JsonProperty("address1")]                      public string Address1                                  { get; set; }
        [JsonProperty("address2")]                      public string Address2                                  { get; set; }
        [JsonProperty("postalCode")]                    public string PostalCode                                { get; set; }
        [JsonProperty("city")]                          public string City                                      { get; set; }
        [JsonProperty("stateCode")]                     public string StateCode                                 { get; set; }
        [JsonProperty("countryCode")]                   public string CountryCode                               { get; set; }
        [JsonProperty("phone")]                         public string Phone                                     { get; set; }
        [JsonProperty("type")]                          public string Type                                      { get; set; }
        [JsonProperty("displayValue")]                  public string DisplayValue                              { get; set; }
    }
}
