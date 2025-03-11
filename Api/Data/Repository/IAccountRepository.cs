using BankingWebApp.Api.Data.Models;
using MongoDB.Driver;

namespace BankingWebApp.Api.Data.Repository
{
    public interface IAccountRepository
    {
        public Task<List<AccountModel>> GetAllAsync();
        public Task<AccountModel> GetByIbanAsync(string iban);
        public Task AddAsync(AccountModel account);
        public Task UpdateAsync(AccountModel account, IClientSessionHandle session = null);
    }
}
