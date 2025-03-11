using AutoMapper;
using BankingWebApp.Api.Data;
using BankingWebApp.Api.Data.Enums;
using BankingWebApp.Api.Data.Models;
using BankingWebApp.Api.Data.Repository;
using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Exceptions;
using BankingWebApp.Api.Services.Impl;
using System.Security.Claims;

namespace BankingWebApp.Api.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDbContext _context;
        private readonly ILogger<TransactionService> _logger;
        private readonly IMapper _mapper;

        public TransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IDbContext context,
            ILogger<TransactionService> logger,
            IMapper mapper)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<TransactionResponse> DepositAsync(TransactionRequest request, List<Claim> claims)
        {
            _logger.LogInformation("Depositing funds: Iban={Iban}, Amount={Amount}",
                request.Iban, request.Amount);
            
            ValidateAccountOWner(request.Iban, claims);
            var account = await GetAccount(request.Iban);

            using (var session = await _context.Client.StartSessionAsync())
            {
                session.StartTransaction();
                account.Balance += request.Amount;
                try
                {
                    await _accountRepository.UpdateAsync(account, session);
                    var transaction = new TransactionModel
                    {
                        ExecutionAccount = request.Iban,
                        Amount = request.Amount,
                        Timestamp = DateTime.UtcNow,
                        Type = TransactionType.Deposit
                    };

                    await _transactionRepository.AddAsync(transaction, session);
                    await session.CommitTransactionAsync();

                    _logger.LogInformation("Funds deposited successfully: Iban={Iban}, Amount={Amount}",
                        request.Iban, request.Amount);

                    return GetTransactionResponse(transaction, account);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during deposit transaction.");

                    await session.AbortTransactionAsync();

                    throw;
                }
            }
        }

        public async Task<TransactionResponse> TransferAsync(TransferTransactionRequest request, List<Claim> claims)
        {
            _logger.LogInformation(
                "Transferring funds: FromAccount={FromAccountIban}, ToAccount={ToAccountIban}, Amount={Amount}",
                request.FromAccount, request.ToAccount, request.Amount);
            
            ValidateAccountOWner(request.FromAccount, claims);
            var fromAccount = await GetAccount(request.FromAccount);

            ValidateBalance(request.FromAccount, fromAccount.Balance, request.Amount);

            var toAccount = await GetAccount(request.ToAccount);

            using (var session = await _context.Client.StartSessionAsync())
            {
                session.StartTransaction();
                fromAccount.Balance -= request.Amount;
                toAccount.Balance += request.Amount;
                try
                {
                    await _accountRepository.UpdateAsync(fromAccount, session);
                    await _accountRepository.UpdateAsync(toAccount, session);

                    var transaction = new TransactionModel
                    {
                        ExecutionAccount = request.FromAccount,
                        ToAccount = request.ToAccount,
                        Amount = request.Amount,
                        Timestamp = DateTime.UtcNow,
                        Type = TransactionType.Transfer
                    };

                    await _transactionRepository.AddAsync(transaction, session);
                    await session.CommitTransactionAsync();

                    _logger.LogInformation("Transfer successful.");

                    return GetTransactionResponse(transaction, fromAccount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during transfer transaction.");

                    await session.AbortTransactionAsync();

                    throw;
                }
            }
        }

        public async Task<TransactionResponse> WithdrawAsync(TransactionRequest request, List<Claim> claims)
        {
            _logger.LogInformation(
                "Withdrawing funds: Iban={Iban}, Amount={Amount}",
                request.Iban, request.Amount);
            
            ValidateAccountOWner(request.Iban, claims);
            var account = await GetAccount(request.Iban);

            ValidateBalance(request.Iban, account.Balance, request.Amount);

            using (var session = await _context.Client.StartSessionAsync())
            {
                session.StartTransaction();
                account.Balance -= request.Amount;
                try
                {
                    await _accountRepository.UpdateAsync(account, session);

                    var transaction = new TransactionModel
                    {
                        ExecutionAccount = request.Iban,
                        Amount = -request.Amount,
                        Timestamp = DateTime.UtcNow,
                        Type = TransactionType.Withdrawal
                    };

                    await _transactionRepository.AddAsync(transaction, session);
                    await session.CommitTransactionAsync();

                    _logger.LogInformation("Funds withdrawn successfully: Iban={Iban}, Amount={Amount}",
                        request.Iban, request.Amount);

                    return GetTransactionResponse(transaction, account);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during withdrawal transaction.");

                    await session.AbortTransactionAsync();

                    throw;
                }
            }
        }

        public TransactionResponse GetTransactionResponse(TransactionModel transaction, AccountModel account)
        {
            var response = _mapper.Map<TransactionResponse>(transaction);
            response.ExecutionAccountDetails = _mapper.Map<AccountDetailsResponse>(account);

            return response;
        }

        private async Task<AccountModel> GetAccount(string iban)
        {
            var account = await _accountRepository.GetByIbanAsync(iban);

            if (account == null)
            {
                _logger.LogError("Account {Iban} not found.", iban);

                throw new AccountNotFoundException(iban);
            }

            return account;
        }

        private void ValidateAccountOWner(string iban, List<Claim> claims)
        {
            var userIban = claims.FirstOrDefault(c => c.Type == "Iban")?.Value;

            if (userIban != iban)
            {
                _logger.LogError("User {Iban} does not have sufficient permissions to execute transaction {RequestedIban}", userIban, iban);

                throw new InvalidOperationException(
                    $"You do not have sufficient permissions to execute transaction on {iban} account. Please provide your Iban to execute transaction.");
            }
        }

        private void ValidateBalance(string iban, decimal balance, decimal amount)
        {
            if (balance < amount)
            {
                _logger.LogError("Insufficient funds: FromAccount={FromAccountIban}, Amount={Amount}",
                    iban, amount);

                throw new InsufficientFundsException();
            }
        }
    }
}