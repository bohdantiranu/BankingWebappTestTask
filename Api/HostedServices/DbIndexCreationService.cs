using BankingWebApp.Api.Data;

namespace BankingWebApp.Api.HostedServices
{
    public class DbIndexCreationService : IHostedService, IDisposable
    {
        private readonly IDbContext _context;
        private readonly ILogger<DbIndexCreationService> _logger;

        public DbIndexCreationService(IDbContext context, ILogger<DbIndexCreationService>  logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{MongoDbIndexCreationService} is starting", nameof(DbIndexCreationService));

            try
            {
                await _context.CreateIndexesAsync();

                _logger.LogInformation("Indexes created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating indexes.");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{MongoDbIndexCreationService} is stopping.", nameof(DbIndexCreationService));

            return Task.CompletedTask;
        }

        public void Dispose() 
        {
            _logger.LogInformation("{MongoDbIndexCreationService} is disposing.", nameof(DbIndexCreationService));
            
            _context?.Dispose();
        }
    }
}
