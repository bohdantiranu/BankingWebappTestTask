using Newtonsoft.Json;

namespace BankingWebApp.Api.Dto.Responses
{
    public class BaseResponse<T>
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public ErrorResponse Error { get; set; }
    }
}
