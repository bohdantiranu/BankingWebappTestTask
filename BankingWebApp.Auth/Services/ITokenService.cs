namespace BankingWebApp.Auth.Services
{
    public interface ITokenService
    {
        public string GenerateToken( string role, string iban = null);
    }
}