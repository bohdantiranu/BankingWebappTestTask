using BankingWebApp.Api.Configurations;
using BankingWebApp.Api.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace BankingWebApp.Api.Data
{
    public class MongoDbContext : IDbContext
    {
        public IMongoClient Client { get; private set; }

        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<MongoDbSettings> configuration, ILogger<MongoDbContext> logger)
        {
            
            Client = new MongoClient(configuration.Value.ConnectionString);
            _database = Client.GetDatabase(configuration.Value.DatabaseName);
            _logger = logger;
        }

        public MongoDbContext(){ }

        public IMongoCollection<AccountModel> Accounts => _database.GetCollection<AccountModel>("Accounts");

        public IMongoCollection<TransactionModel> Transactions => _database.GetCollection<TransactionModel>("Transactions");

        public async Task CreateIndexesAsync()
        {
            _logger.LogInformation("Starting to create indexes.");

            var indexKeys = Builders<AccountModel>.IndexKeys.Ascending(a => a.Iban);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<AccountModel>(indexKeys, indexOptions);

            await Accounts.Indexes.CreateOneAsync(indexModel);

            _logger.LogInformation("Indexes have been created.");
        }

        public void Dispose()
        {
            _logger.LogInformation("MongoDbContext is disposing.");

            try
            {
                Client?.Cluster?.Dispose();

                if (Client?.Cluster?.Description?.State == ClusterState.Disconnected)
                    _logger.LogInformation("MongoDbContext connection closed successfully.");
                else
                    _logger.LogWarning("MongoDbContext connection was not closed properly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing MongoDbContext.");
            }
            finally
            {
                Client = null;

                _logger.LogInformation("MongoDbContext disposed successfully.");
            }
            
        }
    }
}