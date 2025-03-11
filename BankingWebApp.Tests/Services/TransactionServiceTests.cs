using AutoMapper;
using BankingWebApp.Api.Data;
using BankingWebApp.Api.Data.Enums;
using BankingWebApp.Api.Data.Models;
using BankingWebApp.Api.Data.Repository;
using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Exceptions;
using BankingWebApp.Api.Mapping;
using BankingWebApp.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using System.Security.Claims;

namespace BankingWebApp.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<ILogger<TransactionService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly TransactionService _transactionService;
        private readonly Mock<IClientSessionHandle> _mockSession;

        public TransactionServiceTests()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockLogger = new Mock<ILogger<TransactionService>>();
            _mockSession = new Mock<IClientSessionHandle>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()));

            _mockSession.Setup(s => s.StartTransaction(It.IsAny<TransactionOptions>())).Verifiable();
            _mockSession
                .Setup(s => s.CommitTransactionAsync(default))
                .Returns(Task.CompletedTask);
            _mockSession
                .Setup(s => s.AbortTransactionAsync(default))
                .Returns(Task.CompletedTask);
            
            var mockClient = new Mock<IMongoClient>();
            mockClient
                .Setup(c => c.StartSessionAsync(default, default))
                .ReturnsAsync(_mockSession.Object);
            
            var mockContext = new Mock<IDbContext>();
            mockContext
                .Setup(c => c.Client)
                .Returns(mockClient.Object);

            _transactionService = new TransactionService(
                _mockAccountRepository.Object,
                _mockTransactionRepository.Object,
                mockContext.Object,
                _mockLogger.Object,
                _mapper);
        }


        [Fact]
        public async Task DepositAsync_ReturnsTransactionResponse()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 100);
            var claims = CreateClaims(ownerIban);
            var account = CreateAccount(ownerIban, 0);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(ownerIban))
                .ReturnsAsync(account);

            // Act
            var result = await _transactionService.DepositAsync(request, claims);

            // Assert
            result.Should().BeEquivalentTo(
                CreateTransactionResponse(ownerIban, 
                TransactionType.Deposit.ToString(), 
                100, 
                CreateAccountDetailsResponse( account)), 
                o => o.Excluding(r => r.Timestamp));
            _mockAccountRepository.Verify(repo => repo.UpdateAsync(account, _mockSession.Object), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<TransactionModel>(), _mockSession.Object), Times.Once);
            _mockSession.Verify(s => s.StartTransaction(It.IsAny<TransactionOptions>()), Times.Once);
            _mockSession.Verify(s => s.CommitTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DepositAsync_InvalidAccountOwner_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = CreateTransactionRequest("UA123", 100);
            var claims = CreateClaims("UA456");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _transactionService.DepositAsync(request, claims));
            
            _mockAccountRepository.Verify(repo => 
            repo.UpdateAsync(It.IsAny<AccountModel>(), It.IsAny<IClientSessionHandle>()), Times.Never);
            
            _mockTransactionRepository.Verify(repo => 
            repo.AddAsync(It.IsAny<TransactionModel>(), It.IsAny<IClientSessionHandle>()), Times.Never);
        }

        [Fact]
        public async Task DepositAsync_AccountNotFound_ThrowsAccountNotFoundException()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 100);
            var claims = CreateClaims(ownerIban);

            // Act & Assert
            await Assert.ThrowsAsync<AccountNotFoundException>(() => _transactionService.DepositAsync(request, claims));
        }

        [Fact]
        public async Task DepositAsync_TransactionFailure_ThrowsExceptionAndRollsBack()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 100);
            var account = CreateAccount(ownerIban, 0);
            var claims = CreateClaims(ownerIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(ownerIban))
                .ReturnsAsync(account);
            _mockAccountRepository
                .Setup(repo => repo.UpdateAsync(account, _mockSession.Object))
                .ThrowsAsync(new Exception("Simulated transaction failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _transactionService.DepositAsync(request, claims));
            _mockSession.Verify(s => s.AbortTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task TransferAsync_ReturnsTransactionResponse()
        {
            // Arrange
            var fromIban = "UA123";
            var toIban = "UA456";
            var request = CreateTransferTransactionRequest(fromIban, toIban, 50);
            var fromAccount = CreateAccount(fromIban, 100);
            var toAccount = CreateAccount(toIban, 0);
            var claims = CreateClaims(fromIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(fromIban))
                .ReturnsAsync(fromAccount);
            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(toIban))
                .ReturnsAsync(toAccount);

            // Act
            var result = await _transactionService.TransferAsync(request, claims);

            // Assert
            result.Should().BeEquivalentTo(
                CreateTransactionResponse(fromIban,
                TransactionType.Transfer.ToString(),
                50,
                CreateAccountDetailsResponse(fromAccount)),
                o => o.Excluding(r => r.Timestamp));
            _mockAccountRepository.Verify(repo => repo.UpdateAsync(fromAccount, _mockSession.Object), Times.Once);
            _mockAccountRepository.Verify(repo => repo.UpdateAsync(toAccount, _mockSession.Object), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<TransactionModel>(), _mockSession.Object), Times.Once);
            _mockSession.Verify(s => s.StartTransaction(It.IsAny<TransactionOptions>()), Times.Once);
            _mockSession.Verify(s => s.CommitTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task TransferAsync_InvalidAccountOwner_ThrowsInvalidOperationException()
        {
            // Arrange
            var fromIban = "UA123";
            var toIban = "UA456";
            var request = CreateTransferTransactionRequest(fromIban, toIban, 50); 
            var claims = CreateClaims(toIban);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _transactionService.TransferAsync(request, claims));
            _mockAccountRepository.Verify(repo => repo.UpdateAsync(It.IsAny<AccountModel>(), It.IsAny<IClientSessionHandle>()), Times.Never);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<TransactionModel>(), It.IsAny<IClientSessionHandle>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_InsufficientFunds_ThrowsInsufficientFundsException()
        {
            // Arrange
            var fromIban = "UA123";
            var toIban = "UA456";
            var request = CreateTransferTransactionRequest(fromIban, toIban, 150);
            var fromAccount = CreateAccount(fromIban, 100);
            var toAccount = CreateAccount(toIban, 0);
            var claims = CreateClaims(fromIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(fromIban))
                .ReturnsAsync(fromAccount);
            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(toIban))
                .ReturnsAsync(toAccount);

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientFundsException>(() => _transactionService.TransferAsync(request, claims));
        }

        [Fact]
        public async Task TransferAsync_TransactionFailure_ThrowsExceptionAndRollsBack()
        {
            // Arrange
            var fromIban = "UA123";
            var toIban = "UA456";
            var request = CreateTransferTransactionRequest(fromIban, toIban, 50);
            var fromAccount = CreateAccount(fromIban, 100);
            var toAccount = CreateAccount(toIban, 0);
            var claims = CreateClaims(fromIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(fromIban))
                .ReturnsAsync(fromAccount);
            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(toIban))
                .ReturnsAsync(toAccount);
            _mockAccountRepository
                .Setup(repo => repo.UpdateAsync(fromAccount, _mockSession.Object))
                .ThrowsAsync(new Exception("Simulated transaction failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _transactionService.TransferAsync(request, claims));
            _mockSession.Verify(s => s.AbortTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_ReturnsTransactionResponse()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 50);
            var account = CreateAccount(ownerIban, 100);
            var claims = CreateClaims(ownerIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(ownerIban))
                .ReturnsAsync(account);

            // Act
            var result = await _transactionService.WithdrawAsync(request, claims);

            // Assert
            result.Should().BeEquivalentTo(
                CreateTransactionResponse(ownerIban,
                TransactionType.Withdrawal.ToString(),
                -50,
                CreateAccountDetailsResponse(account)),
                o => o.Excluding(r => r.Timestamp));
            _mockAccountRepository.Verify(repo => repo.UpdateAsync(account, _mockSession.Object), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<TransactionModel>(), _mockSession.Object), Times.Once);
            _mockSession.Verify(s => s.StartTransaction(It.IsAny<TransactionOptions>()), Times.Once);
            _mockSession.Verify(s => s.CommitTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_InvalidAccountOwner_ThrowsInvalidOperationException()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 50);
            var claims = CreateClaims("UA456");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _transactionService.WithdrawAsync(request, claims));
            _mockAccountRepository.Verify(repo => repo.UpdateAsync(It.IsAny<AccountModel>(), It.IsAny<IClientSessionHandle>()), Times.Never);
            _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<TransactionModel>(), It.IsAny<IClientSessionHandle>()), Times.Never);
        }

        [Fact]
        public async Task WithdrawAsync_InsufficientFunds_ThrowsInsufficientFundsException()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 150);
            var account = CreateAccount(ownerIban, 100);
            var claims = CreateClaims(ownerIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(ownerIban))
                .ReturnsAsync(account);

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientFundsException>(() => _transactionService.WithdrawAsync(request, claims));
        }

        [Fact]
        public async Task WithdrawAsync_TransactionFailure_ThrowsExceptionAndRollsBack()
        {
            // Arrange
            var ownerIban = "UA123";
            var request = CreateTransactionRequest(ownerIban, 50);
            var account = CreateAccount(ownerIban, 100);
            var claims = CreateClaims(ownerIban);

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(ownerIban))
                .ReturnsAsync(account);
            _mockAccountRepository
                .Setup(repo => repo.UpdateAsync(account, _mockSession.Object))
                .ThrowsAsync(new Exception("Simulated transaction failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _transactionService.WithdrawAsync(request, claims));
            _mockSession.Verify(s => s.AbortTransactionAsync(CancellationToken.None), Times.Once);
        }

        private List<Claim> CreateClaims(string iban) => new List<Claim> { new Claim("Iban", iban) };

        private TransactionRequest CreateTransactionRequest(string iban, decimal amount)
        {
            return new TransactionRequest 
            { 
                Iban = iban, 
                Amount = amount 
            };
        }

        private TransferTransactionRequest CreateTransferTransactionRequest(string fromIban, string toIban,
            decimal amount)
        {
            return new TransferTransactionRequest
            {
                FromAccount = fromIban,
                ToAccount = toIban,
                Amount = amount
            };
        }

        private AccountModel CreateAccount(string iban, decimal balance)
        {
            return new AccountModel 
            { 
                Iban = iban, 
                Balance = balance 
            };
        }

        private TransactionResponse CreateTransactionResponse(string iban, string transactionType, decimal amount, AccountDetailsResponse accountDetails)
        {
            return new TransactionResponse
            {
                TransactionType = transactionType,
                Amount = amount,
                ExecutionAccountDetails = accountDetails
            };
        }

        private AccountDetailsResponse CreateAccountDetailsResponse(AccountModel account)
        {
            return new AccountDetailsResponse
            {
                Iban = account.Iban,
                Balance = account.Balance
            };
        }
    }
}
