using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Requests
{
    public class CreateAccountRequest
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }
}
