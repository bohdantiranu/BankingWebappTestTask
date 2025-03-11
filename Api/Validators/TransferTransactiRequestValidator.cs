using BankingWebApp.Api.Dto.Requests;
using FluentValidation;

namespace BankingWebApp.Api.Validators
{
    public class TransferTransactiRequestValidator : AbstractValidator<TransferTransactionRequest>
    {
        public TransferTransactiRequestValidator()
        {
            RuleFor(x => x.FromAccount)
                .NotEmpty()
                .WithMessage("Sender Iban is required.");
            
            RuleFor(x => x.ToAccount)
                .NotEmpty()
                .WithMessage("Recipient Iban is required.");
            
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");
            
            RuleFor(x => x.FromAccount)
                .NotEqual(x => x.ToAccount)
                .WithMessage("Sender and recipient accounts must be different.");
        }
    }
}
