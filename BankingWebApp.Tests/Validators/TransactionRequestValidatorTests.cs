using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Validators;
using FluentValidation.TestHelper;

namespace BankingWebApp.Tests.Validators
{
    public class TransactionRequestValidatorTests
    {
        private readonly TransactionRequestValidator _validator;

        public TransactionRequestValidatorTests()
        {
            _validator = new TransactionRequestValidator();
        }

        [Fact]
        public void IbanIsEmpty_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransactionRequest(string.Empty, 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Iban)
                .WithErrorMessage("Iban is required.");
        }

        [Fact]
        public void AmountIsZero_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransactionRequest("UA123", 0);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Amount must be greater than 0.");
        }

        [Fact]
        public void AmountIsNegative_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransactionRequest("UA123", -10);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Amount must be greater than 0.");
        }

        [Fact]
        public void RequestIsValid_ShouldNotHaveError()
        {
            // Arrange
            var request = CreateTransactionRequest("UA123", 10);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        private TransactionRequest CreateTransactionRequest(string iban, decimal amount)
        {
            return new TransactionRequest
            {
                Iban = iban,
                Amount = amount
            };
        }
    }
}
