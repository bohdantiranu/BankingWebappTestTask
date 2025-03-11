using BankingWebApp.Auth.Configurations;
using BankingWebApp.Auth.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankingWebApp.Auth.Services
{
    internal class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateToken(string role, string iban = null)
        {
            _logger.LogInformation("Generating token for role: {Role}", role);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                
            };

            if (role == Roles.User && iban != null)
                claims.Add(new Claim($"Iban", iban));

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);
            
            _logger.LogInformation("Token successfully generated for role: {Role}", role);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
