using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.WebV2;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService.Apprenticeship
{
    public class ApprenticeshipBulkUploadServiceTests
    {
        private ApprenticeshipBulkUploadService _apprenticeshipBulkUploadService;

        private readonly AuthUserDetails _authUserDetails = new AuthUserDetails { UKPRN = "666" };
        private readonly Mock<IApprenticeshipService> _mockApprenticeshipService = new Mock<IApprenticeshipService>();
        private readonly Mock<IStandardsAndFrameworksCache> _standardsAndFrameworksCacheMock = new Mock<IStandardsAndFrameworksCache>();
        private readonly Mock<IVenueService> _mockVenueService = new Mock<IVenueService>();

        [Fact]
        public async void TestCountCsvLines()
        {
            SetupDependencies();
            SetupService();
            await using var csvStream = new ApprenticeshipCsvBuilder()
                .WithRow(row => row.WithStandardCode())
                .WithRow(row => row.WithStandardCode()) // duplicate row isn't valid, but count doesn't currently check that
                .BuildStream();

            var actualLineCount = _apprenticeshipBulkUploadService.CountCsvLines(csvStream);

            const int headerRowLines = 1;
            const int dataRowLines = 2;
            Assert.Equal(headerRowLines + dataRowLines, actualLineCount);
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_WithoutFrameworkOrStandard_Throws()
        {
            await Run_ThrowsTest<NullReferenceException>(
                builder => builder
                    .WithRow(row => row
                        .With("STANDARD_CODE", "")
                        .With("STANDARD_VERSION", "")
                        .With("FRAMEWORK_CODE", "")
                        .With("FRAMEWORK_PROG_TYPE", "")
                        .With("FRAMEWORK_PATHWAY_CODE", "")),
                "Object reference not set to an instance of an object.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_WithStandardCode_Success()
        {
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode()),
                ValidateSingleApprenticeshipWithNoErrors);
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_WithFrameworkCode_Success()
        {
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithFrameworkCode()),
                ValidateSingleApprenticeshipWithNoErrors);
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_WithRegions_Success()
        {
            // region values from Legacy\Dfc.CourseDirectory.Models\Models\Regions\SelectRegionModel.cs
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row
                        .WithFrameworkCode()
                        .With("DELIVERY_METHOD", "Employer")
                        .With("NATIONAL_DELIVERY", "No")
                        .With("REGION", "North West;London")
                        .With("SUB_REGION", "Darlington;Hammersmith and Fulham")),
                (apprenticeships) =>
                {
                    var apprenticeship = ValidateAndReturnSingleApprenticeshipWithNoErrors(apprenticeships);
                    Assert.Single(apprenticeship.ApprenticeshipLocations);
                    Assert.Equal(
                        new List<string>
                        {
                            "E12000002", // North West
                            "E12000007", // London
                            "E06000002", // Darlington
                            // todo: where did Hammersmith and Fulham go?
                        },
                        apprenticeship.ApprenticeshipLocations.Single().Regions);
                });
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_Success_WithUnixLineEndings()
        {
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode())
                    .WithUnixLineEndings(),
                ValidateSingleApprenticeshipWithNoErrors);
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_Success_WithoutTrailingNewline()
        {
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode())
                    .WithoutTrailingNewline(),
                ValidateSingleApprenticeshipWithNoErrors);
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_InvalidHeader_Throws()
        {
            await Run_ThrowsTest<HeaderValidationException>(
                builder => builder
                    .WithRow(row => row.WithStandardCode())
                    .WithInvalidHeader(),
                "Header with name 'STANDARD_CODE' was not found. If you are expecting some headers" +
                " to be missing and want to ignore this validation, set the configuration HeaderValidated" +
                " to null. You can also change the functionality to do something else, like logging the issue.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_DuplicateRow_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row.WithStandardCode())
                    .WithRow(row => row.WithStandardCode()),
                "Duplicate entries detected on rows 2, and 3.");
            // todo: test semi-colon separated list of duplicate pairs
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_EmptyFile_Throws()
        {
            await Run_ThrowsTest<Exception>(
                builder => builder.EmptyFile(),
                "File is empty.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_NoRows_Throws()
        {
            await Run_ThrowsTest<Exception>(
                builder => builder.WithoutRows(),
                "The selected file is empty");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_ArchiveFailure_Throws()
        {
            const string fakedErrorMessage = "failure message from ChangeApprenticeshipStatusesForUKPRNSelection here";

            await Run_ThrowsTest<Exception>(
                builder => builder.WithRow(),
                expectedErrorMessage: fakedErrorMessage,
                additionalMockSetup: () =>
                {
                    _mockApprenticeshipService.Setup(m =>
                            m.ChangeApprenticeshipStatusesForUKPRNSelection(It.IsAny<int>(), It.IsAny<int>(),
                                It.IsAny<int>()))
                        .ReturnsAsync(Result.Fail(fakedErrorMessage));
                });
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_WithoutStandardCode_WithStandardVersion_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row
                        .With("STANDARD_CODE", "")
                        .With("STANDARD_VERSION", "1")
                    ),
                "Validation error on row 2. Missing Standard Code.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_StandardCode_NonNumeric_Throws()
        {
            await Run_ThrowsTest<FieldValidationException>(
                builder => builder.WithRow(
                    row => row.With("STANDARD_CODE", "this-is-not-an-integer")),
                "Validation error on row 2. Field STANDARD_CODE must be numeric if present.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_StandardVersion_Blank_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row.With("STANDARD_CODE","123")),
                "Validation error on row 2. Missing Standard Version.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_StandardVersion_NonNumeric_Throws()
        {
            await Run_ThrowsTest<FieldValidationException>(
                builder => builder.WithRow(
                    row => row.With("STANDARD_VERSION", "this-is-definitely-not-an-integer")),
                "Validation error on row 2. Field STANDARD_VERSION must be numeric if present.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_StandardCode_Unknown_Throws()
        {
            const int code = 404404;
            const int version = 9;
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row
                        .With("STANDARD_CODE", code.ToString())
                        .With("STANDARD_VERSION", version.ToString())
                    ),
                expectedErrorMessage: "Validation error on row 2. Invalid Standard Code or Version Number. Standard not found.",
                additionalMockSetup: () =>
                {
                    _standardsAndFrameworksCacheMock.Setup(m => m.GetStandard(code,version))
                        .ReturnsAsync((Standard)null);
                });
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_FrameworkCode_Unknown_Throws()
        {
            const int code = 404404;
            const int type = 44;
            const int pathway = 4;
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row
                        .With("FRAMEWORK_CODE", code.ToString())
                        .With("FRAMEWORK_PROG_TYPE", type.ToString())
                        .With("FRAMEWORK_PATHWAY_CODE", pathway.ToString())
                    ),
                expectedErrorMessage: "Validation error on row 2. Invalid Framework Code, Prog Type, or Pathway Code. Framework not found.",
                additionalMockSetup: () =>
                {
                    _standardsAndFrameworksCacheMock.Setup(m => m.GetFramework(code, type, pathway))
                        .ReturnsAsync((Framework)null);
                });
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_FrameworkWithoutCode_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row
                        .With("FRAMEWORK_CODE", "")
                        .With("FRAMEWORK_PROG_TYPE", "101")
                        .With("FRAMEWORK_PATHWAY_CODE", "101")
                    ),
                "Validation error on row 2. Missing Framework Code.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_FrameworkWithoutType_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row
                        .With("FRAMEWORK_CODE", "101")
                        .With("FRAMEWORK_PROG_TYPE", "")
                        .With("FRAMEWORK_PATHWAY_CODE", "101")
                    ),
                "Validation error on row 2. Missing Prog Type.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_FrameworkWithoutPathway_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder
                    .WithRow(row => row
                        .With("FRAMEWORK_CODE", "101")
                        .With("FRAMEWORK_PROG_TYPE", "101")
                        .With("FRAMEWORK_PATHWAY_CODE", "")
                    ),
                "Validation error on row 2. Missing Pathway Type.");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_DeliveryMethod_Blank_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder.WithRow(
                    row => row.With("DELIVERY_METHOD", "   ")),
                "Validation error on row 2. DELIVERY_METHOD is required.");
        }

        [Theory]
        [InlineData("CONTACT_PHONE", "Telephone is required")]
        [InlineData("CONTACT_EMAIL", "Email is required")]
        public async Task TestValidateAndUploadCSV_RequiredFieldBlank_ReturnsErrors(string field, string expectedError)
        {
            await Run_ReturnsErrorsTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With(field, "")),
                expectedError);
        }

        [Theory]
        [InlineData("APPRENTICESHIP_WEBPAGE", "example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "http://example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "https://example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE",  "https://xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.org")]
        [InlineData("CONTACT_URL", "example.org")]
        [InlineData("CONTACT_URL", "http://example.org")]
        [InlineData("CONTACT_URL", "https://example.org")]
        public async Task TestValidateAndUploadCSV_UrlField_Success(string field, string url)
        {
            await Run_SuccessTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With(field, url)),
                ValidateSingleApprenticeshipWithNoErrors);
        }

        [Theory]
        [InlineData("APPRENTICESHIP_WEBPAGE", "x@example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "exampleorg")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "xxx")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "x.x")]
        [InlineData("CONTACT_URL", "x@example.org")]
        [InlineData("CONTACT_URL", "exampleorg")]
        [InlineData("CONTACT_URL", "xxx")]
        [InlineData("CONTACT_URL", "x.x")]
        public async Task TestValidateAndUploadCSV_UrlFields_Invalid_StoresErrors(string field, string url)
        {
            await Run_SuccessTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With(field, url)),
                apprenticeships =>
                {
                    ValidateBulkUploadError(apprenticeships,
                        field,
                        $"Validation error on row 2. Field {field} format of URL is incorrect.");
                });
        }

        [Theory]
        [InlineData("APPRENTICESHIP_WEBPAGE")]
        [InlineData("CONTACT_URL")]
        public async Task TestValidateAndUploadCSV_UrlFields_TooLong_StoresErrors(string field)
        {
            const string longUrl = "https://xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.org";
            await Run_SuccessTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With(field, longUrl)),
                apprenticeships =>
                {
                    ValidateBulkUploadError(apprenticeships,
                        field,
                        $"Validation error on row 2. Field {field} maximum length is 255 characters.");
                });
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_ApprenticeshipInfo_Blank_ReturnsErrors()
        {
            await Run_ReturnsErrorsTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With("APPRENTICESHIP_INFORMATION", "")),
                "Marketing Information is required");
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_ApprenticeshipInfo_TooLong_StoresError()
        {
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("APPRENTICESHIP_INFORMATION", new string('x', 100000))),
                apprenticeships =>
                {
                    ValidateBulkUploadError(apprenticeships,
                        "APPRENTICESHIP_INFORMATION",
                        "Validation error on row 2. Field APPRENTICESHIP_INFORMATION maximum length is 750 characters.");
                });
        }

        private void SetupService()
        {
            _apprenticeshipBulkUploadService = new ApprenticeshipBulkUploadService(
                NullLogger<ApprenticeshipBulkUploadService>.Instance, _mockApprenticeshipService.Object,
                _mockVenueService.Object, _standardsAndFrameworksCacheMock.Object);
        }

        private void SetupDependencies()
        {
            var mockVenue = new Venue
            {
                VenueName = ApprenticeshipCsvRowBuilder.DefaultTestVenueName,
                Status = VenueStatus.Live,
            };

            _mockApprenticeshipService.Setup(m =>
                    m.ChangeApprenticeshipStatusesForUKPRNSelection(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            _mockVenueService.Setup(m => m.SearchAsync(It.IsAny<IVenueSearchCriteria>()))
                .ReturnsAsync(Result.Ok<IVenueSearchResult>(new VenueSearchResult(new List<Venue> {mockVenue})));

            _standardsAndFrameworksCacheMock.Setup(m => m.GetStandard(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Standard());

            _standardsAndFrameworksCacheMock.Setup(m => m.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Framework());
        }

        private async Task Run_SuccessTest(Action<ApprenticeshipCsvBuilder> configureCsv,
            Action<IList<IApprenticeship>> validateDataPassedToApprenticeshipService)
        {
            // arrange
            SetupDependencies();
            IList<IApprenticeship> dataPassedToApprenticeshipService = null;
            _mockApprenticeshipService.Setup(m => m.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>()))
                .ReturnsAsync(Result.Ok())
                .Callback<IEnumerable<IApprenticeship>>(x => dataPassedToApprenticeshipService = x.ToList());
            SetupService();
            var apprenticeshipCsvBuilder = new ApprenticeshipCsvBuilder();
            configureCsv?.Invoke(apprenticeshipCsvBuilder);
            await using var csvStream = apprenticeshipCsvBuilder.BuildStream();

            // act
            var actualErrors = await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                csvStream, _authUserDetails, updateApprenticeships: true); // todo: toggle updateApprenticeships in test(s)

            // assert
            var emptyErrorList = new List<string>();
            Assert.Equal(emptyErrorList, actualErrors);
            validateDataPassedToApprenticeshipService(dataPassedToApprenticeshipService);
        }

        private async Task Run_ThrowsTest<TException>(
                Action<ApprenticeshipCsvBuilder> configureCsv,
                string expectedErrorMessage,
                Action additionalMockSetup = null)
            where TException : Exception
        {
            // arrange
            SetupDependencies();
            _mockApprenticeshipService.Setup(m => m.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>()))
                .ReturnsAsync(Result.Ok());
            additionalMockSetup?.Invoke();
            SetupService();
            var apprenticeshipCsvBuilder = new ApprenticeshipCsvBuilder();
            configureCsv?.Invoke(apprenticeshipCsvBuilder);
            await using var csvStream = apprenticeshipCsvBuilder.BuildStream();

            // act
            var actualException = await Record.ExceptionAsync(
                async () => await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                    csvStream, _authUserDetails, updateApprenticeships: true) // todo: toggle updateApprenticeships in test(s)
            );

            // assert
            Assert.NotNull(actualException);
            Assert.IsType<TException>(actualException);
            Assert.Equal(expectedErrorMessage, actualException.Message);
        }

        private async Task<List<IApprenticeship>> Run_ReturnsErrorsTest(Action<ApprenticeshipCsvBuilder> configureCsv, string expectedError)
        {
            // arrange
            SetupDependencies();
            List<IApprenticeship> dataPassedToApprenticeshipService = null;
            _mockApprenticeshipService.Setup(m => m.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>()))
                .ReturnsAsync(Result.Ok())
                .Callback<IEnumerable<IApprenticeship>>(x => dataPassedToApprenticeshipService = x.ToList());
            SetupService();
            var apprenticeshipCsvBuilder = new ApprenticeshipCsvBuilder();
            configureCsv?.Invoke(apprenticeshipCsvBuilder);
            await using var csvStream = apprenticeshipCsvBuilder.BuildStream();

            // act
            var actualErrors = await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                csvStream, _authUserDetails, updateApprenticeships: true); // todo: toggle updateApprenticeships in test(s)

            // assert
            var expectedErrors = new List<string> {expectedError};
            Assert.Equal(expectedErrors, actualErrors);
            return dataPassedToApprenticeshipService;
        }

        private static void ValidateSingleApprenticeshipWithNoErrors(IList<IApprenticeship> apprenticeships)
        {
            ValidateAndReturnSingleApprenticeshipWithNoErrors(apprenticeships);
        }

        private static IApprenticeship ValidateAndReturnSingleApprenticeshipWithNoErrors(IList<IApprenticeship> apprenticeships)
        {
            Assert.Single(apprenticeships);
            Assert.Null(apprenticeships.Single().ValidationErrors);
            Assert.Empty(apprenticeships.Single().BulkUploadErrors);
            return apprenticeships.Single();
        }

        private static void ValidateBulkUploadError(IList<IApprenticeship> apprenticeships, string fieldName, string expectedError)
        {
            Assert.Single(apprenticeships);
            var apprenticeship = apprenticeships.Single();
            Assert.Null(apprenticeship.ValidationErrors);
            Assert.Single(apprenticeship.BulkUploadErrors);
            var bulkUploadError = apprenticeship.BulkUploadErrors.Single();
            Assert.Equal(fieldName, bulkUploadError.Header);
            Assert.Equal(expectedError, bulkUploadError.Error);
            Assert.Equal(2, bulkUploadError.LineNumber);
        }
    }
}
