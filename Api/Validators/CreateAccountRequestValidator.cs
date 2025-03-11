using BankingWebApp.Api.Dto.Requests;
using FluentValidation;

namespace BankingWebApp.Api.Validators
{
    public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
    {
        public CreateAccountRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First Name is required");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last Name is required");

            RuleFor(x => x.Balance)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Balance can`t be negative");
        }
    }
}
