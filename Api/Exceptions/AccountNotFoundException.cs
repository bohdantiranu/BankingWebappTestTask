namespace BankingWebApp.Api.Exceptions
{
    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException(string iban) : base($"Account with number {iban} not found.") 
        {
        }
    }
}
