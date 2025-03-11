using BankingWebApp.Api.Data.Models;
using MongoDB.Driver;

namespace BankingWebApp.Api.Data.Repository
{
    public interface ITransactionRepository
    {
        public Task<List<TransactionModel>> GetAllAsync();
        public Task AddAsync(TransactionModel transaction, IClientSessionHandle session = null);
        public Task UpdateAsync(TransactionModel transaction);
    }
}
