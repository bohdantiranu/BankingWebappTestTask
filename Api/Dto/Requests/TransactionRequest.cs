using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Requests
{
    public class TransactionRequest
    {
        [JsonProperty("accountIban")]
        public string Iban { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
