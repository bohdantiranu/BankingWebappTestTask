namespace BankingWebApp.Api.Exceptions
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException() : base("Insufficient funds.") 
        { 
        }
    }
}
