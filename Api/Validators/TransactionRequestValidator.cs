using BankingWebApp.Api.Dto.Requests;
using FluentValidation;

namespace BankingWebApp.Api.Validators
{
    public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
    {
        public TransactionRequestValidator()
        {
            RuleFor(x => x.Iban)
                .NotEmpty()
                .WithMessage("Iban is required.");
           
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");
        }
    }
}
