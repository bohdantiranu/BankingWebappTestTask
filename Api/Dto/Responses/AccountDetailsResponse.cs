using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Responses
{
    public class AccountDetailsResponse
    {
        [JsonProperty("iban")]
        public string Iban { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }
}
