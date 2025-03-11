namespace BankingWebApp.Auth.Configurations
{
    internal class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
