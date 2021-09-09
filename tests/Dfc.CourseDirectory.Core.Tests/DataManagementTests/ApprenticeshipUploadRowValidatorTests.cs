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
                DeliveryModes = "employer address",
                DeliveryMethod = "classroom based"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == "Enter apprenticeship information for employers");
        }


        [Theory]
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("876")]
        public async Task InvalidRadiusRange_ReturnsValidationError(string radius)
        {
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = string.Empty,
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                Radius = radius
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == "You must enter a radius between 1 and 874");
        }


        [Fact]
        public async Task ApprenticeshipInformationLargerThanMaxLength_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx2222222222222222222",
                DeliveryModes = "employer address",
                DeliveryMethod = "classroom based"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

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
                DeliveryModes = "employer address",
                DeliveryMethod = "classroom based"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Website must be a real webpage");
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
                DeliveryModes = "employer address",
                DeliveryMethod = "classroom based",
                ContactEmail = "@invalid.com"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Enter an email address in the correct format");
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
                DeliveryModes = "employer address",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "INVALID NUMBER"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"Enter a telephone number in the correct format");
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
                DeliveryModes = "employer address",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "invalid"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorMessage == $"The contact us webpage must be a real website");
        }

        [Fact]
        public async Task ApprenticeshipMissingRadiusWhenEmployerBased_DoesNotReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "employer address",
                DeliveryMethod = "employer based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "",
                //Regions

            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_RADIUS_REQUIRED");
        }

        [Fact]
        public async Task ApprenticeshipMissingRadiusWhenClassroomBased_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "",
                //Regions

            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_RADIUS_REQUIRED");
        }

        [Theory]
        [InlineData("day release")]
        [InlineData("block release")]
        public async Task ApprenticeshipWheEmployerBasedAndSetsDayOrBlockRelease_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = deliveryMode,
                DeliveryMethod = "employer based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
                //Regions
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_DELIVERYMODE_NOT_ALLOWED");
        }

        [Theory]
        [InlineData("employer address")]
        [InlineData(null)]
        public async Task ApprenticeshipWhenClassroomBasedAndNotDayOrBlockRelease_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = deliveryMode,
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK");
        }

        [Theory]
        [InlineData("day release")]
        [InlineData("block release")]
        public async Task ApprenticeshipWhenClassroomBasedAndDayOrBlockRelease_ReturnsValidResult(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = deliveryMode,
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK");
        }

        [Fact]
        public async Task ApprenticeshipWhenYourVenueReferenceIsValid_ReturnsValidResult()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
                YourVenueReference = "Valid Reference"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_YOUR_VENUE_REFERENCE_MAXLENGTH");
        }

        [Theory]
        [InlineData("yes")]
        [InlineData("no")]
        public async Task ApprenticeshipWhenEmployerBasedAndNationalSet_ReturnsValidResult(string national)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "employer based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
                YourVenueReference = "Valid Reference",
                NationalDelivery = national
                //Regions
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_NATIONALDELIVERY_NOT_ALLOWED");
        }

        [Fact]
        public async Task ApprenticeshipWhenClassroomBasedAndNationalSet_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
                YourVenueReference = "Valid Reference",
                NationalDelivery = "yes"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_NATIONALDELIVERY_NOT_ALLOWED");
        }


        [Fact]
        public async Task ApprenticeshipWhenValidClassroomBased_ReturnsValidResult()
        {
            // Arrange
            var user = await TestData.CreateUser();
            var provider = await TestData.CreateProvider(providerType: Models.ProviderType.Apprenticeships);
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();
            var venue = await TestData.CreateVenue(
                providerId: provider.ProviderId,
                createdBy: user,
                venueName: "My Venue",
                providerVenueRef: "VENUE1");


            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                YourVenueReference = "VENUE1",
                Radius = "10"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: venue.VenueId);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        public async Task ApprenticeshipWhenInvalidVenueRef_ReturnsValidationError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: Models.ProviderType.Apprenticeships);
            var user = await TestData.CreateUser();
            var venue = await TestData.CreateVenue(
                providerId: provider.ProviderId,
                createdBy: user,
                venueName: "My Venue",
                providerVenueRef: "VENUE1");
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                YourVenueReference = "VENUE2"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_PROVIDER_VENUE_REF_INVALID");
        }

        [Fact]
        public async Task ApprenticeshipYourVenueRefEmptyButVenueNotEmpty_DoesNotReturnValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryModes = "day release",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                YourVenueReference = string.Empty,
                VenueName = "SomeVenue"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.PropertyName == nameof(CsvApprenticeshipRow.YourVenueReference));
        }

        [Fact]
        public async Task ApprenticeshipWhenValidEmployerBased_ReturnsValidResult()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMethod = "employer based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                SubRegion = "County Durham"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        public async Task ApprenticeshipWhenEmployerBasedWithInvalidSubRegion_ReturnsValidResult()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMethod = "employer based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                YourVenueReference = "Valid Reference",
                Radius = "1",
                SubRegion = "SOME INVALID REGION"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_SUBREGIONS_INVALID");
        }

        [Fact]
        public async Task ApprenticeshipWhenEmployerBasedWithoutRegions_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMethod = "employer based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                YourVenueReference = "Valid Reference",
                Radius = "1",
                SubRegion = null,
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_SUBREGIONS_REQUIRED");
        }



        [Fact]
        public async Task ApprenticeshipWhenClassroomBasedWithSubRegions_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                YourVenueReference = "Valid Reference",
                Radius = "1",
                SubRegion = "County Durham"
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_SUBREGIONS_NOT_ALLOWED");
        }


        [Fact]
        public async Task ApprenticeshipWhenClassroomBasedWithEmptyVenueAndVenueReference_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvApprenticeshipRow()
            {
                StandardCode = "1",
                StandardVersion = "1",
                ApprenticeshipInformation = "Some info",
                ApprenticeshipWebpage = "https://someapprenticeship.com",
                DeliveryMethod = "classroom based",
                ContactEmail = "someemail@invalid.com",
                ContactPhone = "0121 111 1111",
                ContactUrl = "https://someapprenticeship.com",
                Radius = "1",
                SubRegion = "County Durham",
                VenueName = string.Empty,
                YourVenueReference = string.Empty
            };

            var validator = new ApprenticeshipUploadRowValidator(Clock, matchedVenueId: null);

            // Act
            var validationResult = validator.Validate(ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "APPRENTICESHIP_VENUE_REQUIRED");
        }

    }
}
