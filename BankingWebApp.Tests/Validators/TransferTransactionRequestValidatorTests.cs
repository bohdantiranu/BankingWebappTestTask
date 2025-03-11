using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Validators;
using FluentValidation.TestHelper;

namespace BankingWebApp.Tests.Validators
{
    public class TransferTransactionRequestValidatorTests
    {
        private readonly TransferTransactiRequestValidator _validator;

        public TransferTransactionRequestValidatorTests()
        {
            _validator = new TransferTransactiRequestValidator();
        }

        [Fact]
        public void FromAccountIsEmpty_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransferTransactionRequest(string.Empty, "UA456", 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FromAccount)
                .WithErrorMessage("Sender Iban is required.");
        }

        [Fact]
        public void ToAccountIsNull_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransferTransactionRequest("UA123", null, 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ToAccount)
                .WithErrorMessage("Recipient Iban is required.");
        }

        [Fact]
        public void AmountIsZero_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransferTransactionRequest("UA123", "UA456", 0);

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
            var request = CreateTransferTransactionRequest("UA123", "UA456", -10);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Amount must be greater than 0.");
        }

        [Fact]
        public void FromAccountEqualsToAccount_ShouldHaveError()
        {
            // Arrange
            var request = CreateTransferTransactionRequest("UA123", "UA123", 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FromAccount)
                .WithErrorMessage("Sender and recipient accounts must be different.");
        }

        [Fact]
        public void RequestIsValid_ShouldNotHaveError()
        {
            // Arrange
            var request = CreateTransferTransactionRequest("UA123", "UA456", 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        private TransferTransactionRequest CreateTransferTransactionRequest(string fromIban, string toIban, 
            decimal amount)
        {
            return new TransferTransactionRequest
            {
                FromAccount = fromIban,
                ToAccount = toIban,
                Amount = amount
            };
        }
    }
}
