using AutoMapper;
using BankingWebApp.Api.Data.Models;
using BankingWebApp.Api.Data.Repository;
using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Mapping;
using BankingWebApp.Api.Services;
using BankingWebApp.Auth.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace BankingWebApp.Tests.Services
{
    public class AccountsServiceTests
    {
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<ILogger<AccountsService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly AccountsService _accountsService;

        public AccountsServiceTests()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockLogger = new Mock<ILogger<AccountsService>>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()));
            _accountsService = new AccountsService(_mockAccountRepository.Object, _mockLogger.Object, _mapper);
        }

        [Fact]
        public async Task CreateAccountAsync_ValidRequest_ReturnsAccountDetailsResponse()
        {
            // Arrange
            _mockAccountRepository
                .Setup(repo => repo.AddAsync(It.IsAny<AccountModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _accountsService.CreateAccountAsync(GetCreateAccountRequest());

            // Assert
            result.Should().NotBeNull();
            _mockAccountRepository.Verify(repo => repo.AddAsync(It.IsAny<AccountModel>()), Times.Once);
        }

        [Fact(Skip = "WriteError has not public constructor")]
        public async Task CreateAccountAsync_DuplicateIban_RetriesAndReturnsAccountDetailsResponse()
        {
            // Arrange
            _mockAccountRepository
                .Setup(repo => repo.AddAsync(It.IsAny<AccountModel>()))
                .ThrowsAsync(new Exception());
            
            _mockAccountRepository
                .Setup(repo => repo.AddAsync(It.IsAny<AccountModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _accountsService.CreateAccountAsync(GetCreateAccountRequest());

            // Assert
            result.Should().NotBeNull();
            _mockAccountRepository.Verify(repo => repo.AddAsync(It.IsAny<AccountModel>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task CreateAccountAsync_MaxRetriesExceeded_ThrowsException()
        {
            // Arrange
            _mockAccountRepository
                .Setup(repo => repo.AddAsync(It.IsAny<AccountModel>()))
                .ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
            _accountsService.CreateAccountAsync(GetCreateAccountRequest()));
        }

        [Fact]
        public async Task GetAccountByIbanAsync_AdminRole_ReturnsAccountDetailsResponse()
        {
            // Arrange
            var iban = "UA12305299123456789";
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, Roles.Admin) };

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(iban))
                .ReturnsAsync(GetAccount());

            // Act
            var result = await _accountsService.GetAccountByIbanAsync(iban, claims);

            // Assert
            result.Should().BeEquivalentTo(GetAccountDetailsResponse());
        }

        [Fact]
        public async Task GetAccountByIbanAsync_UserRole_ReturnsAccountDetailsResponse()
        {
            // Arrange
            var iban = "UA12305299123456789";
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, Roles.User), new Claim("Iban", iban) };

            _mockAccountRepository
                .Setup(repo => repo.GetByIbanAsync(iban))
                .ReturnsAsync(GetAccount());

            // Act
            var result = await _accountsService.GetAccountByIbanAsync(iban, claims);

            // Assert
            result.Should().BeEquivalentTo(GetAccountDetailsResponse());
        }

        [Fact]
        public async Task GetAccountByIbanAsync_UserRoleDifferentIban_ThrowsInvalidOperationException()
        {
            // Arrange
            var iban = "UA12305299123456789";
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, Roles.User), new Claim("Iban", "UA98765") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _accountsService.GetAccountByIbanAsync(iban, claims));
        }

        [Fact]
        public async Task GetAllAccountsAsync_ReturnsListOfAccountDetailsResponse()
        {
            // Arrange
            var accounts = new List<AccountModel> 
            { 
                new AccountModel { Iban = "UA123" }, 
                new AccountModel { Iban = "UA456" } 
            };
            var accountDetailsResponses = new List<AccountDetailsResponse> 
            { 
                new AccountDetailsResponse { Iban = "UA123" }, 
                new AccountDetailsResponse { Iban = "UA456" } 
            };

            _mockAccountRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(accounts);

            // Act
            var result = await _accountsService.GetAllAccountsAsync();

            // Assert
            result.Should().BeEquivalentTo(accountDetailsResponses);
        }

        private CreateAccountRequest GetCreateAccountRequest()
        {
            return new CreateAccountRequest 
            { 
                FirstName = "Ololo", 
                LastName = "Ololoychenko", 
                Balance = 100 
            };
        }

        private AccountModel GetAccount()
        {
            return new AccountModel 
            { 
                Iban = "UA12305299123456789", 
                FirstName = "Ololo", 
                LastName = "Ololoychenko", 
                Balance = 100 
            };
        }
        
        private AccountDetailsResponse GetAccountDetailsResponse()
        {
            return new AccountDetailsResponse
            {
                Iban = "UA12305299123456789",
                FirstName = "Ololo",
                LastName = "Ololoychenko",
                Balance = 100
            };
        }
    } 
}
