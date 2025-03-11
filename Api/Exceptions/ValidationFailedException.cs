using FluentValidation.Results;

namespace BankingWebApp.Api.Exceptions
{
    public class ValidationFailedException : Exception
    {
        public List<ValidationFailure> Errors { get; }

        public ValidationFailedException(List<ValidationFailure> errors) : base("Validation failed.")
        {
            Errors = errors;
        }
    }
}
