using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Requests
{
    public class TransferTransactionRequest
    {
        [JsonProperty("accountIban")]
        public string FromAccount { get; set; }

        [JsonProperty("recipientAccountIban")]
        public string ToAccount { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
