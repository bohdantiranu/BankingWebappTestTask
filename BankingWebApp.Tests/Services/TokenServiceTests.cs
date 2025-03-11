using BankingWebApp.Auth.Configurations;
using BankingWebApp.Auth.Models;
using BankingWebApp.Auth.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankingWebApp.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private readonly Mock<ILogger<TokenService>> _mockLogger;
        private readonly TokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public TokenServiceTests()
        {
            _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
            _mockLogger = new Mock<ILogger<TokenService>>();

            _jwtSettings = new JwtSettings
            {
                Key = "ThisIsASecretKeyForTestingPurposes",
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            };

            _mockJwtSettings
                .Setup(s => s.Value)
                .Returns(() => _jwtSettings);

            _tokenService = new TokenService(_mockJwtSettings.Object, _mockLogger.Object);
        }

        [Fact]
        public void GenerateToken_UserRoleWithIban_ShouldGenerateValidToken()
        {
            // Arrange
            var iban = "UA123";

            // Act
            var token = _tokenService.GenerateToken(Roles.User, iban);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Should().NotBeNull();
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == Roles.User);
            jwtToken.Claims.Should().Contain(c => c.Type == "Iban" && c.Value == iban);
            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
            jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateToken_AdminRole_ShouldGenerateValidToken()
        {
            // Arrange

            // Act
            var token = _tokenService.GenerateToken(Roles.Admin);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Should().NotBeNull();
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == Roles.Admin);
            jwtToken.Claims.Should().NotContain(c => c.Type == "Iban");
            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
            jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
        }
    }
}
