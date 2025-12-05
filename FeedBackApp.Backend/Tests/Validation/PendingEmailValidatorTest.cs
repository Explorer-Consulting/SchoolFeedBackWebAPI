using Application.DTOs.Email;
using Application.Validation.Email;
using FluentAssertions;

namespace Tests.Validation
{
    /// <summary>
    /// Unit tests for PendingEmailValidator demonstrating how to test FluentValidation validators.
    /// These tests show how validation failures are returned and how to verify specific validation rules.
    /// </summary>
    [TestFixture]
    public class PendingEmailValidatorTest
    {
        private PendingEmailValidator _validator = null!;

        [SetUp]
        public void Setup()
        {
            _validator = new PendingEmailValidator();
        }

        [Test]
        public async Task ValidateAsync_WithValidDto_ReturnsValidResult()
        {
            // Arrange
            var validDto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = "Test Survey",
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(validDto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public async Task ValidateAsync_WithEmptySurveyId_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = string.Empty,
                SurveyName = "Test Survey",
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SurveyId");
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Survey ID cannot be empty"));
        }

        [Test]
        public async Task ValidateAsync_WithInvalidSurveyIdFormat_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = "not-a-valid-guid",
                SurveyName = "Test Survey",
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "SurveyId" && 
                e.ErrorMessage.Contains("Survey ID must be a valid GUID format"));
        }

        [Test]
        public async Task ValidateAsync_WithEmptySurveyName_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = string.Empty,
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "SurveyName" && 
                e.ErrorMessage.Contains("Survey name cannot be empty"));
        }

        [Test]
        public async Task ValidateAsync_WithTooShortSurveyName_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = "AB", // Less than 3 characters
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "SurveyName" && 
                e.ErrorMessage.Contains("must be at least 3 characters long"));
        }

        [Test]
        public async Task ValidateAsync_WithTooLongSurveyName_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = new string('A', 201), // More than 200 characters
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "SurveyName" && 
                e.ErrorMessage.Contains("cannot exceed 200 characters"));
        }

        [Test]
        public async Task ValidateAsync_WithInvalidEmail_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = "Test Survey",
                Email = "invalid-email",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "Email" && 
                e.ErrorMessage.Contains("Invalid email format"));
        }

        [Test]
        public async Task ValidateAsync_WithEmptyEmail_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = "Test Survey",
                Email = string.Empty,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "Email" && 
                e.ErrorMessage.Contains("Email address cannot be empty"));
        }

        [Test]
        public async Task ValidateAsync_WithEndDateBeforeStartDate_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = "Test Survey",
                Email = "test@example.com",
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today // End date before start date
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("End date must be after start date"));
        }

        [Test]
        public async Task ValidateAsync_WithEndDateSameAsStartDate_ReturnsValidationError()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = Guid.NewGuid().ToString(),
                SurveyName = "Test Survey",
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today // Same date
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("end date must be at least 1 day after start date"));
        }

        [Test]
        public async Task ValidateAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var dto = new PendingEmailDTO
            {
                SurveyId = string.Empty,
                SurveyName = "AB", // Too short
                Email = "invalid-email",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today // Same date
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(1);
            result.Errors.Should().Contain(e => e.PropertyName == "SurveyId");
            result.Errors.Should().Contain(e => e.PropertyName == "SurveyName");
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Test]
        public async Task ValidateAsync_WithValidGuidSurveyId_DoesNotReturnFormatError()
        {
            // Arrange
            var validGuid = Guid.NewGuid().ToString();
            var dto = new PendingEmailDTO
            {
                SurveyId = validGuid,
                SurveyName = "Test Survey",
                Email = "test@example.com",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => 
                e.ErrorMessage.Contains("Survey ID must be a valid GUID format"));
        }
    }
}


