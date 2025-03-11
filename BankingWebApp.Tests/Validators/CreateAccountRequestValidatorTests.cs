using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Validators;
using FluentValidation.TestHelper;

namespace BankingWebApp.Tests.Validators
{
    public class CreateAccountRequestValidatorTests
    {
        private readonly CreateAccountRequestValidator _validator;

        public CreateAccountRequestValidatorTests()
        {
            _validator = new CreateAccountRequestValidator();
        }

        [Fact]
        public void FirstNameIsEmpty_ShouldHaveError()
        {
            // Arrange
            var request = CreateAccountRequest(string.Empty, "Ololoychenko", 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First Name is required");
        }

        [Fact]
        public void LastNameIsEmpty_ShouldHaveError()
        {
            // Arrange
            var request = CreateAccountRequest("Ololo", null, 0);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                .WithErrorMessage("Last Name is required");
        }

        [Fact]
        public void BalanceIsNegative_ShouldHaveError()
        {
            // Arrange
            var request = CreateAccountRequest("Ololo", "Ololoychenko", -100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Balance)
                .WithErrorMessage("Balance can`t be negative");
        }

        [Fact]
        public void RequestIsValid_ShouldNotHaveError()
        {
            // Arrange
            var request = CreateAccountRequest("Ololo", "Ololoychenko", 100);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        private CreateAccountRequest CreateAccountRequest(string firstName, string lastName, decimal balance)
        {
            return new CreateAccountRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Balance = balance
            };
        }
    }
}
