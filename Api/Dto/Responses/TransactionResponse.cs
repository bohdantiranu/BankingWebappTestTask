using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Responses
{
    public class TransactionResponse
    {
        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("executionAccountDetails")]
        public AccountDetailsResponse ExecutionAccountDetails { get; set; }
    }
}
