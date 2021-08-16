using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Xunit;
using static Dfc.CourseDirectory.Core.DataManagement.FileUploadProcessor;
using constants = Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation.Constants;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public class ApprenticeshipUploadRowValidatorTests : DatabaseTestBase
    {
        public ApprenticeshipUploadRowValidatorTests(Testing.DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task ApprenticeshipInformationEmpty_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = string.Empty,
                DeliveryMode = "classroom based",
                DeliveryMethod = "classroom based"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == "Enter apprenticeship information for employers");
        }

        [Fact]
        public async Task ApprenticeshipInformationLargerThanMaxLength_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx2222222222222222222",
                DeliveryMode = "classroom based",
                DeliveryMethod = "classroom based"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Apprenticeship information for employers must be {constants.MarketingInformationStrippedMaxLength} characters or fewer");
        }

        [Fact]
        public async Task ApprenticeshipInvalidApprenticeshipWebpage_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "ttp://someapprenticeship.com",
                DeliveryMode = "classroom based",
                DeliveryMethod = "classroom based"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Website must be a real webpage, like http://www.provider.com/apprenticeship");
        }

        [Fact]
        public async Task ApprenticeshipInvalidContactEmail_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMode = "classroom based",
                DeliveryMethod = "classroom based",
                ContactEmail = "@invalid.com"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Email must be a valid email address");
        }

        [Fact]
        public async Task ApprenticeshipInvalidContactPhoneNumber_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMode = "classroom based",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "INVALID NUMBER"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Telephone must be a valid UK phone number");
        }


        [Fact]
        public async Task ApprenticeshipInvalidContactUrl_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMode = "classroom based",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "invalid"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Contact us page must be a real webpage, like http://www.provider.com/apprenticeship");
        }


    }
}
