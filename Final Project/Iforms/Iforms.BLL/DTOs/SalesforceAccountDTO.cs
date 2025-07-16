namespace Iforms.BLL.DTOs
{
    using System.Text.Json.Serialization;

    public class SalesforceAccountDTO
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        [JsonPropertyName("BillingStreet")]
        public string Address { get; set; }
        [JsonPropertyName("BillingCity")]
        public string City { get; set; }
        [JsonPropertyName("BillingCountry")]
        public string Country { get; set; }
        // Add other Account fields as needed
    }
} 