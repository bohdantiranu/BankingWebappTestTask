using BankingWebApp.Api.Data.Models;
using MongoDB.Driver;

namespace BankingWebApp.Api.Data
{
    public interface IDbContext : IDisposable
    {
        IMongoClient Client { get; }
        IMongoCollection<AccountModel> Accounts { get; }
        IMongoCollection<TransactionModel> Transactions { get; }
        Task CreateIndexesAsync();
    }
}
