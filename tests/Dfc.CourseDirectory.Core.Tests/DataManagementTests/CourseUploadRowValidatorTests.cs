﻿using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.Services;
using Moq;
using Xunit;
using static Dfc.CourseDirectory.Core.DataManagement.FileUploadProcessor;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public class CourseUploadRowValidatorTests : DatabaseTestBase
    {
        public CourseUploadRowValidatorTests(Testing.DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task AttendancePatternEmptyWithClassroomBasedDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                AttendancePattern = string.Empty,
                DeliveryMode = "classroom based"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_ATTENDANCE_PATTERN_REQUIRED");
        }

        [Fact]
        public async Task AttendancePatternEmptyWithBlendedLearningDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                AttendancePattern = string.Empty,
                DeliveryMode = "blended learning"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_ATTENDANCE_PATTERN_REQUIRED");
        }

        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task AttendancePatternNotEmptyWithNonClassroomBasedDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                AttendancePattern = "daytime",
                DeliveryMode = deliveryMode
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED");
        }

        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task AttendancePatternNotEmptyWithNonBlendedLearningDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                AttendancePattern = "daytime",
                DeliveryMode = deliveryMode
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED");
        }

        [Fact]
        public async Task CostEmptyWithEmptyCostDescription_ReturnsSingleCostValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                Cost = string.Empty,
                CostDescription = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_COST_REQUIRED");
        }

        [Fact]
        public async Task DurationUnitMissing_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DurationUnit = string.Empty,
                Duration = "10"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_DURATION_UNIT_REQUIRED");
        }

        [Fact]
        public async Task NationalDeliveryEmptyWithWorkBasedDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "work based",
                NationalDelivery = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_NATIONAL_DELIVERY_REQUIRED");
        }

        [Theory]
        [InlineData("yes")]
        [InlineData("no")]
        public async Task NationalDeliveryNotEmptyWithNonWorkBasedDeliveryMode_ReturnsValidationError(string nationalDelivery)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                NationalDelivery = nationalDelivery
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_NATIONAL_DELIVERY_NOT_ALLOWED");
        }

        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task ProviderVenueRefNotEmptyWithNonClassroomBasedDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                ProviderVenueRef = "venue"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED");
        }
        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task ProviderVenueRefNotEmptyWithNonBlendedLearningDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                ProviderVenueRef = "venue"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED");
        }
        [Fact]
        public async Task ProviderVenueRefAndVenueNameEmptyWithClassroomBasedDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = string.Empty,
                VenueName = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_VENUE_REQUIRED");
        }
        [Fact]
        public async Task ProviderVenueRefAndVenueNameEmptyWithBlendedLearningDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "blended learning",
                ProviderVenueRef = string.Empty,
                VenueName = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_VENUE_REQUIRED");
        }
        [Fact]
        public async Task ProviderVenueRefInvalid_ReturnsValidationError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var user = await TestData.CreateUser();

            var venue = await TestData.CreateVenue(
                providerId: provider.ProviderId,
                createdBy: user,
                venueName: "My Venue",
                providerVenueRef: "VENUE1");

            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "VENUE2"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_PROVIDER_VENUE_REF_INVALID");
        }

        [Fact]
        public async Task ProviderVenueRefEmptyButVenueNameNotEmpty_DoesNotReturnValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = string.Empty,
                VenueName = "venue"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.PropertyName == nameof(CsvCourseRow.ProviderVenueRef));
        }

        [Fact]
        public async Task StartDateEmptyWithFlexibleFalse_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                StartDate = string.Empty,
                FlexibleStartDate = "no"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_START_DATE_REQUIRED");
        }

        [Fact]
        public async Task StartDateNotEmptyWithFlexibleTrue_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                StartDate = "01/04/2021",
                FlexibleStartDate = "yes"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_START_DATE_NOT_ALLOWED");
        }

        [Fact]
        public async Task StudyModeEmptyWithClassroomBasedDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                StudyMode = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_STUDY_MODE_REQUIRED");
        }
        [Fact]
        public async Task StudyModeEmptyWithBlendedLearningDeliveryMode_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "blended learning",
                StudyMode = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_STUDY_MODE_REQUIRED");
        }

        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task StudyModeNotEmptyWithNonClassroomBasedDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                StudyMode = "full time"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_STUDY_MODE_NOT_ALLOWED");
        }

        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task StudyModeNotEmptyWithNonBlendedLearningDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                StudyMode = "full time"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_STUDY_MODE_NOT_ALLOWED");
        }

        [Fact]
        public async Task SubRegionsEmptyWithNationalFalse_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "work based",
                NationalDelivery = "no",
                SubRegions = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_REQUIRED");
        }

        [Theory]
        [InlineData("classroom based")]
        [InlineData("online")]
        [InlineData("xxx")]
        public async Task SubRegionsEmptyWithNationalFalseForNonWorkBasedDeliveryMode_DoesNotReturnRequiredValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                NationalDelivery = "no",
                SubRegions = string.Empty
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_REQUIRED");
        }

        [Fact]
        public async Task SubRegionsNotEmptyWithNationalTrue_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "work based",
                NationalDelivery = "yes",
                SubRegions = "Warwickshire"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_NOT_ALLOWED");
        }

        [Fact]
        public async Task SubRegionsInvalid_ReturnsValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "work based",
                NationalDelivery = "no",
                SubRegions = "x"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_INVALID");
        }

        [Fact]
        public async Task SubRegionsValid_DoesNotReturnValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "work based",
                NationalDelivery = "no",
                SubRegions = "Warwickshire; Derbyshire"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_INVALID");
        }

        [Fact]
        public async Task SubRegionsEmptyWithNational_DoesNotReturnValidationError()
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "work based",
                NationalDelivery = "yes",
                SubRegions = ""
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_INVALID");
        }

        [Theory]
        [InlineData("classroom based")]
        [InlineData("online")]
        public async Task SubRegionsInvalidWithNonWorkBasedDeliveryMode_DoesNotReturnInvalidValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                SubRegions = "xxx"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_SUBREGIONS_INVALID");
        }

        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task VenueNameNotEmptyWithNonClassroomBasedDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                ProviderCourseRef = string.Empty,
                VenueName = "venue"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_VENUE_NAME_NOT_ALLOWED");
        }
        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        public async Task VenueNameNotEmptyWithNonBlendedLearningDeliveryMode_ReturnsValidationError(string deliveryMode)
        {
            // Arrange
            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                ProviderCourseRef = string.Empty,
                VenueName = "venue"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_VENUE_NAME_NOT_ALLOWED");
        }
        [Fact]
        public async Task VenueNameInvalid_ReturnsValidationError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var user = await TestData.CreateUser();

            var venue = await TestData.CreateVenue(
                providerId: provider.ProviderId,
                createdBy: user,
                venueName: "My Venue",
                providerVenueRef: "VENUE1");

            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "bad venue"
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_VENUE_NAME_INVALID");
        }

        [Fact]
        public async Task VenueNameSpecifiedWithProviderVenueRef_ReturnsValidationError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var user = await TestData.CreateUser();

            var venue = await TestData.CreateVenue(
                providerId: provider.ProviderId,
                createdBy: user,
                venueName: "My Venue",
                providerVenueRef: "VENUE1");

            var allRegions = await new RegionCache(SqlQueryDispatcherFactory).GetAllRegions();

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = venue.ProviderVenueRef,
                VenueName = venue.VenueName
            };

            var validator = CreateCourseUploadRowValidator();

            // Act
            var validationResult = validator.Validate(ParsedCsvCourseRow.FromCsvCourseRow(row, allRegions));

            // Assert
            Assert.Contains(
                validationResult.Errors,
                error => error.ErrorCode == "COURSERUN_VENUE_NAME_NOT_ALLOWED_WITH_REF");
        }

        private CourseUploadRowValidator CreateCourseUploadRowValidator()
        {
            var mockedWebRiskService = new Mock<IWebRiskService>();
            mockedWebRiskService.Setup(x => x.CheckForSecureUri(It.IsAny<string>())).ReturnsAsync(true);
            var validator = new CourseUploadRowValidator(Clock, matchedVenueId: null, mockedWebRiskService.Object);
            return validator;
        }
    }
}
