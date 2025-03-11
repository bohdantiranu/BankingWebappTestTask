namespace BankingWebApp.Api.Exceptions
{
    public class TransactionFailedException : Exception
    {
        public TransactionFailedException(string message) : base(message) 
        {
        }

        public TransactionFailedException(string message, Exception innerException) : base(message, innerException) 
        {
        }
    }
}
