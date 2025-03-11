using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using System.Security.Claims;

namespace BankingWebApp.Api.Services.Impl
{
    public interface IAccountsService
    {
        public Task<AccountDetailsResponse> CreateAccountAsync(CreateAccountRequest accountDto);
        public Task<AccountDetailsResponse> GetAccountByIbanAsync(string iban, List<Claim> claims);
        public Task<IEnumerable<AccountDetailsResponse>> GetAllAccountsAsync();
    }
}
