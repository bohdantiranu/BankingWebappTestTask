using AutoMapper;
using BankingWebApp.Api.Data.Models;
using BankingWebApp.Api.Data.Repository;
using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Services.Impl;
using BankingWebApp.Auth.Models;
using MongoDB.Driver;
using System.Security.Claims;

namespace BankingWebApp.Api.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountsService> _logger;
        private readonly IMapper _mapper;

        public AccountsService(
            IAccountRepository accountRepository, 
            ILogger<AccountsService> logger,
            IMapper mapper)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<AccountDetailsResponse> CreateAccountAsync(CreateAccountRequest request)
        {
            _logger.LogInformation("Creating account: FirstName={FirstName}, LastName={LastName}, Balance={Balance}", 
                request.FirstName, request.LastName, request.Balance);

            var account = _mapper.Map<AccountModel>(request);
            const int maxRetry = 10;

            for (int retryCount = 0; retryCount < maxRetry; retryCount++)
            {
                account.Iban = GenerateIban();
                try
                {
                    await _accountRepository.AddAsync(account);
                    
                    return _mapper.Map<AccountDetailsResponse>(account);
                }
                catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    _logger.LogWarning($"Duplicate IBAN generated. Retrying (attempt {retryCount + 1}).");
                }
            }

            throw new Exception("Failed to generate unique IBAN after multiple attempts.");
        }

        public async Task<AccountDetailsResponse> GetAccountByIbanAsync(string iban, List<Claim> claims)
        {
            _logger.LogInformation("Getting account details: Iban={Iban}", iban);
            
            var userRole = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            
            if (userRole != Roles.Admin)
            {
                var userIban = claims.FirstOrDefault(c => c.Type == "Iban")?.Value;
                
                if (userIban != iban) 
                {
                    _logger.LogError("User {Iban} does not have sufficient permissions to get {RequestedIban}", userIban, iban);

                    throw new InvalidOperationException(
                        $"You do not have sufficient permissions to retrieve {iban} account data. Please provide your Iban to get your account details.");
                }
            }
            
            var account = await _accountRepository.GetByIbanAsync(iban);
            
            return _mapper.Map<AccountDetailsResponse>(account);
        }

        public async Task<IEnumerable<AccountDetailsResponse>> GetAllAccountsAsync()
        {
            _logger.LogInformation("Getting all accounts");

            var accounts = await _accountRepository.GetAllAsync();

            return _mapper.Map<List<AccountDetailsResponse>>(accounts);
        }

        private string GenerateIban()
        {
            var random = new Random();

            return "UA" + random.Next(10, 100) + "305299" + random.Next(100000000, int.MaxValue);
        }
    }
}
