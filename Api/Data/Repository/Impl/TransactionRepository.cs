using BankingWebApp.Api.Data.Models;
using MongoDB.Driver;

namespace BankingWebApp.Api.Data.Repository.Impl
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMongoCollection<TransactionModel> _transactions;
        public TransactionRepository(IDbContext context)
        {
            _transactions = context.Transactions;
        }

        public async Task AddAsync(TransactionModel transaction, IClientSessionHandle session = null)
        {
            if (session == null)
                await _transactions.InsertOneAsync(transaction);
            else
                await _transactions.InsertOneAsync(session, transaction);
        }

        public async Task<List<TransactionModel>> GetAllAsync() => await _transactions.Find(_ => true).ToListAsync();

        public async Task UpdateAsync(TransactionModel transaction) => 
            await _transactions.ReplaceOneAsync(x => x.Id == transaction.Id, transaction);
    }
}
