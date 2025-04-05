using FluentValidation.TestHelper;
using IncidentManagement.Application.Validation;
using Xunit;

namespace IncidentManagement.Application.Tests
{
    public class CreateIncidentCommandValidatorTests
    {
        private readonly CreateIncidentCommandValidator _validator;

        public CreateIncidentCommandValidatorTests()
        {
            _validator = new CreateIncidentCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldPass()
        {
            // Arrange
            var command = new CreateIncidentCommand(
                "Valid Name",
                "Valid Description"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyName_ShouldFail()
        {
            // Arrange
            var command = new CreateIncidentCommand(
                "",
                "Valid Description"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_WithTooLongName_ShouldFail()
        {
            // Arrange
            var command = new CreateIncidentCommand(
                new string('x', 101),
                "Valid Description"
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_WithTooLongDescription_ShouldFail()
        {
            // Arrange
            var command = new CreateIncidentCommand(
                "Valid Name",
                new string('x', 501)
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}