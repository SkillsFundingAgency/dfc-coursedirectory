using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Options = Microsoft.Extensions.Options.Options;
using VenueStatus = Dfc.CourseDirectory.Models.Models.Venues.VenueStatus;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService.Apprenticeship
{
    public class ApprenticeshipBulkUploadServiceTests
    {
        private const string DefaultVenue = ApprenticeshipCsvRowBuilder.DefaultTestVenueName;
        private ApprenticeshipBulkUploadService _apprenticeshipBulkUploadService;

        private readonly AuthUserDetails _authUserDetails = new AuthUserDetails { UKPRN = "666" };
        private readonly Mock<IApprenticeshipService> _mockApprenticeshipService = new Mock<IApprenticeshipService>();
        private readonly Mock<IStandardsAndFrameworksCache> _standardsAndFrameworksCacheMock = new Mock<IStandardsAndFrameworksCache>();
        private readonly Mock<IVenueService> _mockVenueService = new Mock<IVenueService>();
        private readonly Mock<IBinaryStorageProvider> _mockBinaryStorageProvider = new Mock<IBinaryStorageProvider>();
        private readonly IOptions<ApprenticeshipBulkUploadSettings> _settings = Options.Create(new ApprenticeshipBulkUploadSettings()
        {
            ProcessSynchronouslyRowLimit = 100
        });
        private bool _backgroundWorkScheduled;

        private List<Venue> _mockVenues = new List<Venue>
        {
            new Venue {VenueName = DefaultVenue, Status = VenueStatus.Live},
        };

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
        public async Task TestValidateAndUploadCSV_WithFrameworkCode_IsIgnored()
        {
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithFrameworkCode()),
                apps => Assert.Empty(apps));
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_WithRegions_Success()
        {
            // region values from Legacy\Dfc.CourseDirectory.Models\Models\Regions\SelectRegionModel.cs
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row
                        .WithStandardCode()
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


        [ClassData(typeof(LocationVariationFacts))]
        [Theory]
        public async Task TestValidateAndUploadCSV_LocationVariations(LocationVariationFact fact)
        {
            var csvBuilder =
                new Action<ApprenticeshipCsvBuilder>(builder => builder
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("DELIVERY_METHOD", fact.DeliveryMethod)
                        .With("DELIVERY_MODE", fact.DeliveryMode)
                        .With("ACROSS_ENGLAND", fact.AcrossEngland)
                        .With("NATIONAL_DELIVERY", fact.NationalDelivery)
                        .With("VENUE", fact.Venue)
                        .With("REGION", fact.Region)
                        .With("SUB_REGION", fact.SubRegion)
                    )
                );

            if (fact.ExpectedError == null)
            {
                IApprenticeship output = null;
                await Run_SuccessTest(
                    csvBuilder,
                    (f) => output = ValidateAndReturnSingleApprenticeshipWithNoErrors(f));
                Assert.Equal(fact.ExpectedOutputDeliveryMode ?? default, (ApprenticeshipDeliveryMode?)output.ApprenticeshipLocations.SingleOrDefault()?.DeliveryModes?.SingleOrDefault());
            }
            else
            {
                await Run_ThrowsTest<BadDataException>(
                    csvBuilder,
                    fact.ExpectedError);
            }
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
        public async Task TestValidateAndUploadCSV_DeliveryMethod_Blank_Throws()
        {
            await Run_ThrowsTest<BadDataException>(
                builder => builder.WithRow(
                    row => row.With("DELIVERY_METHOD", "   ")),
                "Validation error on row 2. DELIVERY_METHOD is required.");
        }

        [InlineData("CONTACT_PHONE", "Telephone is required")]
        [InlineData("CONTACT_EMAIL", "Email is required")]
        [Theory]
        public async Task TestValidateAndUploadCSV_RequiredFieldBlank_ReturnsErrors(string field, string expectedError)
        {
            await Run_ReturnsErrorsTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With(field, "")),
                expectedError);
        }

        [InlineData("APPRENTICESHIP_WEBPAGE", "example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "http://example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "https://example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE",  "https://xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.org")]
        [InlineData("CONTACT_URL", "example.org")]
        [InlineData("CONTACT_URL", "http://example.org")]
        [InlineData("CONTACT_URL", "https://example.org")]
        [Theory]
        public async Task TestValidateAndUploadCSV_UrlField_Success(string field, string url)
        {
            await Run_SuccessTest(
                builder => builder.WithRow(row => row
                    .WithStandardCode()
                    .With(field, url)),
                ValidateSingleApprenticeshipWithNoErrors);
        }

        [InlineData("APPRENTICESHIP_WEBPAGE", "x@example.org")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "exampleorg")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "xxx")]
        [InlineData("APPRENTICESHIP_WEBPAGE", "x.x")]
        [InlineData("CONTACT_URL", "x@example.org")]
        [InlineData("CONTACT_URL", "exampleorg")]
        [InlineData("CONTACT_URL", "xxx")]
        [InlineData("CONTACT_URL", "x.x")]
        [Theory]
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

        [InlineData("APPRENTICESHIP_WEBPAGE")]
        [InlineData("CONTACT_URL")]
        [Theory]
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

        [Fact]
        public async Task TestValidateAndUploadCSV_AggregatesMultipleLocations_WithStandardCode()
        {
            var mockVenue1 = AddMockVenue("mockVenueS1");
            var mockVenue2 = AddMockVenue("mockVenueS2");
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("VENUE", mockVenue1.VenueName)
                    )
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("VENUE", mockVenue2.VenueName)
                    ),
                apprenticeships =>
                {
                    Assert.Equal(2, apprenticeships.Single().ApprenticeshipLocations.Count);
                }
            );
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_SplitsLocationsByDeliveryMethod()
        {
            var mockVenue1 = AddMockVenue("mockVenue1");
            var mockVenue2 = AddMockVenue("mockVenue2");
            var mockVenue3 = AddMockVenue("mockVenue3");
            var mockVenue4 = AddMockVenue("mockVenue4");
            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("DELIVERY_METHOD", "employer")
                        .With("NATIONAL_DELIVERY", "yes") // required for DELIVERY_METHOD=employer, not part of test
                        .With("VENUE", mockVenue1.VenueName)
                    )
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("DELIVERY_METHOD", "employer")
                        .With("NATIONAL_DELIVERY", "yes") // required for DELIVERY_METHOD=employer, not part of test
                        .With("VENUE", mockVenue2.VenueName)
                    )
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("DELIVERY_METHOD", "classroom")
                        .With("VENUE", mockVenue3.VenueName)
                    )
                    .WithRow(row => row
                        .WithStandardCode()
                        .With("DELIVERY_METHOD", "classroom")
                        .With("VENUE", mockVenue4.VenueName)
                    ),
                apprenticeships =>
                {
                    Assert.Equal(2, apprenticeships.Count);
                    Assert.Equal(2, apprenticeships.First().ApprenticeshipLocations.Count);
                    Assert.Equal(2, apprenticeships.Skip(1).Single().ApprenticeshipLocations.Count);
                }
            );
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_ProcessedSynchronously_UploadsFile()
        {
            // Ensure limit is >= row count so file is processed synchronously
            _settings.Value.ProcessSynchronouslyRowLimit = 2;

            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode()),
                validateDataPassedToApprenticeshipService: _ => { },  // no need to validate here
                fileName: "myfile.csv");

            _mockBinaryStorageProvider.Verify(m => m.UploadFile(
                It.Is<string>(f => f.Contains("myfile.csv")),
                It.IsAny<Stream>()));
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_NotProcessedSynchronously_UploadsFile()
        {
            // Ensure limit is < row count so file is *not* processed synchronously
            _settings.Value.ProcessSynchronouslyRowLimit = 1;

            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode())
                    .WithRow(row => row.WithFrameworkCode()),
                validateDataPassedToApprenticeshipService: _ => { },  // no need to validate here
                fileName: "myfile.csv");

            _mockBinaryStorageProvider.Verify(m => m.UploadFile(
                It.Is<string>(f => f.Contains("myfile.csv")),
                It.IsAny<Stream>()));
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_ProcessedSynchronously_UploadsApprenticeshipsSynchronously()
        {
            // Ensure limit is >= row count so file is processed synchronously
            _settings.Value.ProcessSynchronouslyRowLimit = 2;

            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode()),
                validateDataPassedToApprenticeshipService: _ => { },  // no need to validate here
                fileName: "myfile.csv");

            Assert.False(_backgroundWorkScheduled);
        }

        [Fact]
        public async Task TestValidateAndUploadCSV_NotProcessedSynchronously_UploadsApprenticeshipsAsynchronously()
        {
            // Ensure limit is < row count so file is *not* processed synchronously
            _settings.Value.ProcessSynchronouslyRowLimit = 1;

            await Run_SuccessTest(
                builder => builder
                    .WithRow(row => row.WithStandardCode())
                    .WithRow(row => row.WithFrameworkCode()),
                validateDataPassedToApprenticeshipService: _ => { },  // no need to validate here
                fileName: "myfile.csv");

            Assert.True(_backgroundWorkScheduled);
        }

        private void SetupService()
        {
            // Need to be able to resolve the service via IApprenticeshipBulkUploadService
            var serviceProvider = new ServiceCollection()
                .AddTransient<IApprenticeshipBulkUploadService>(_ => _apprenticeshipBulkUploadService)
                .BuildServiceProvider();

            // Use a background scheduler that executes work items immediately
            // and wrap that in a `SignallingBackgroundWorkScheduler` so we can observe the scheduling of work items
            var backgroundWorkScheduler = new SignallingBackgroundWorkScheduler(
                new ExecuteImmediatelyBackgroundWorkScheduler(serviceProvider.GetRequiredService<IServiceScopeFactory>()),
                onScheduled: () => _backgroundWorkScheduled = true);

            _apprenticeshipBulkUploadService = new ApprenticeshipBulkUploadService(
                NullLogger<ApprenticeshipBulkUploadService>.Instance, _mockApprenticeshipService.Object,
                _mockVenueService.Object, _standardsAndFrameworksCacheMock.Object, _mockBinaryStorageProvider.Object,
                backgroundWorkScheduler,
                _settings);
        }

        private void SetupDependencies()
        {
            _mockApprenticeshipService.Setup(m =>
                    m.ChangeApprenticeshipStatusesForUKPRNSelection(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            _mockVenueService.Setup(m => m.SearchAsync(It.IsAny<IVenueSearchCriteria>()))
                .ReturnsAsync(Result.Ok<IVenueSearchResult>(new VenueSearchResult(_mockVenues)));

            _standardsAndFrameworksCacheMock.Setup(m => m.GetStandard(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((int s, int v) => new Standard {StandardCode = s, Version = v});

            _standardsAndFrameworksCacheMock.Setup(m => m.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((int c, int t, int p) => new Framework{FrameworkCode = c, ProgType = t, PathwayCode = p});
        }

        private async Task Run_SuccessTest(
            Action<ApprenticeshipCsvBuilder> configureCsv,
            Action<IList<IApprenticeship>> validateDataPassedToApprenticeshipService,
            string fileName = "mybulkupload.csv")
        {
            // arrange
            SetupDependencies();
            IList<IApprenticeship> dataPassedToApprenticeshipService = null;
            _mockApprenticeshipService.Setup(m => m.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>(), It.IsAny<bool>()))
                .ReturnsAsync(Result.Ok())
                .Callback<IEnumerable<IApprenticeship>, bool>((x, _) => dataPassedToApprenticeshipService = x.ToList());
            SetupService();
            var apprenticeshipCsvBuilder = new ApprenticeshipCsvBuilder();
            configureCsv?.Invoke(apprenticeshipCsvBuilder);
            await using var csvStream = apprenticeshipCsvBuilder.BuildStream();

            // act
            var result = await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                fileName, csvStream, _authUserDetails);

            // assert
            var actualErrors = result.Errors;
            Assert.Empty(actualErrors);
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
            _mockApprenticeshipService.Setup(m => m.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>(), It.IsAny<bool>()))
                .ReturnsAsync(Result.Ok());
            additionalMockSetup?.Invoke();
            SetupService();
            var apprenticeshipCsvBuilder = new ApprenticeshipCsvBuilder();
            configureCsv?.Invoke(apprenticeshipCsvBuilder);
            await using var csvStream = apprenticeshipCsvBuilder.BuildStream();

            // act
            var actualException = await Record.ExceptionAsync(
                async () => await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                    "mybulkupload.csv", csvStream, _authUserDetails)
            );

            // assert
            Assert.NotNull(actualException);
            Assert.IsType<TException>(actualException);
            Assert.Equal(expectedErrorMessage, actualException.Message);
        }

        private async Task<List<IApprenticeship>> Run_ReturnsErrorsTest(
            Action<ApprenticeshipCsvBuilder> configureCsv,
            string expectedError)
        {
            // arrange
            SetupDependencies();
            List<IApprenticeship> dataPassedToApprenticeshipService = null;
            _mockApprenticeshipService.Setup(m => m.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>(), It.IsAny<bool>()))
                .ReturnsAsync(Result.Ok())
                .Callback<IEnumerable<IApprenticeship>, bool>((x, _) => dataPassedToApprenticeshipService = x.ToList());
            SetupService();
            var apprenticeshipCsvBuilder = new ApprenticeshipCsvBuilder();
            configureCsv?.Invoke(apprenticeshipCsvBuilder);
            await using var csvStream = apprenticeshipCsvBuilder.BuildStream();

            // act
            var result = await _apprenticeshipBulkUploadService.ValidateAndUploadCSV(
                "mybulkupload.csv", csvStream, _authUserDetails);

            // assert
            var actualErrors = result.Errors;
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

        private Venue AddMockVenue(string venueName)
        {
            var mockVenue = new Venue
            {
                VenueName = venueName,
                Status = VenueStatus.Live,
            };
            _mockVenues.Add(mockVenue);
            return mockVenue;
        }
    }
}
