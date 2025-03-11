using BankingWebApp.Api.Configurations;
using BankingWebApp.Api.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BankingWebApp.Api.Data.Repository.Impl
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<AccountModel> _accounts;

        public AccountRepository(IDbContext context)
        {
            _accounts = context.Accounts;
        }

        public async Task AddAsync(AccountModel account) => await _accounts.InsertOneAsync(account);

        public async Task<List<AccountModel>> GetAllAsync() => await _accounts.Find(_ => true).ToListAsync();

        public async Task<AccountModel> GetByIbanAsync(string iban) => 
            await _accounts.Find(x => x.Iban == iban).FirstOrDefaultAsync();

        public async Task UpdateAsync(AccountModel account, IClientSessionHandle session = null)
        {
            if (session == null)
                await _accounts.ReplaceOneAsync(x => x.Iban == account.Iban, account);
            else
                await _accounts.ReplaceOneAsync(session, x => x.Iban == account.Iban, account);
        }
    }
}