using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Responses
{
    public class ErrorResponse
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }
    }
}
