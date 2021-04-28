using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public partial class FileUploadProcessorTests
    {
        [Fact]
        public async Task GetVenueUploadStatusUpdates_VenueUploadDoesNotExist_ReturnsArgumentException()
        {
            // Arrange
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            var venueUploadId = Guid.NewGuid();

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdates(venueUploadId);

            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => { }, cts.Token);
            uploadProcessor.ReleaseUploadStatusCheck();
            uploadProcessor.OnComplete();
            var error = await Record.ExceptionAsync(() => completed);

            error.Should().BeOfType<ArgumentException>()
                .Subject.Message.Should().StartWith("Specified venue upload does not exist.");
        }

        [Fact]
        public async Task GetVenueUploadStatusUpdates_EmitsInitialStatus()
        {
            // Arrange
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdates(venueUpload.VenueUploadId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            uploadProcessor.ReleaseUploadStatusCheck();
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.First().Should().Be(UploadStatus.Created);
        }

        [Fact]
        public async Task GetVenueUploadStatusUpdates_EmitsChangedStatus()
        {
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdates(venueUpload.VenueUploadId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            uploadProcessor.ReleaseUploadStatusCheck();  // Created
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, UploadStatus.Processing);
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, UploadStatus.ProcessedSuccessfully);
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created, UploadStatus.Processing, UploadStatus.ProcessedSuccessfully });
        }

        [Fact]
        public async Task GetVenueUploadStatusUpdates_DoesNotEmitDuplicateStatuses()
        {
            // Arrange
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdates(venueUpload.VenueUploadId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            uploadProcessor.ReleaseUploadStatusCheck();
            uploadProcessor.ReleaseUploadStatusCheck();
            uploadProcessor.ReleaseUploadStatusCheck();
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created });
        }

        [Theory]
        [InlineData(UploadStatus.Abandoned)]
        [InlineData(UploadStatus.Published)]
        public async Task GetVenueUploadStatusUpdates_CompletesWhenStatusIsTerminal(UploadStatus uploadStatus)
        {
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdates(venueUpload.VenueUploadId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            uploadProcessor.ReleaseUploadStatusCheck();  // Created
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, uploadStatus);
            uploadProcessor.OnComplete();
            await completed;
        }

        [Fact]
        public async Task MatchRowsToExistingVenues_MatchesOnRef()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var venue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, providerVenueRef: "ref");

            var rows = new[] { new VenueRow() { ProviderVenueRef = "REF", VenueName = "NAME" } };

            var existingVenues = new[] { venue1 };

            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            // Act
            var result = uploadProcessor.MatchRowsToExistingVenues(rows, existingVenues);

            // Assert
            result[0].Should().Be(venue1.VenueId);
        }

        [Fact]
        public async Task MatchRowsToExistingVenues_MatchesOnName()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var venue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "name");

            var rows = new[] { new VenueRow() { VenueName = "NAME" } };

            var existingVenues = new[] { venue1 };

            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            // Act
            var result = uploadProcessor.MatchRowsToExistingVenues(rows, existingVenues);

            // Assert
            result[0].Should().Be(venue1.VenueId);
        }

        [Fact]
        public async Task MatchRowsToExistingVenues_CandidateCannotBeMatchedMoreThanOnce()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var venue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "name", providerVenueRef: "ref");

            var rows = new[]
            {
                new VenueRow() { VenueName = "NAME2", ProviderVenueRef = "REF" },
                new VenueRow() { VenueName = "NAME" },
            };

            var existingVenues = new[] { venue1 };

            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(SqlQueryDispatcherFactory, Clock);

            // Act
            var result = uploadProcessor.MatchRowsToExistingVenues(rows, existingVenues);

            // Assert
            result[0].Should().Be(venue1.VenueId);
            result[1].Should().BeNull();
        }

        [Fact]
        public async Task ProcessVenueFile_AllRecordsValid_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var uploadRows = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 3).ToArray();
            await WithSqlQueryDispatcher(dispatcher => AddPostcodeInfoForRows(dispatcher, uploadRows));
            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessVenueFile(venueUpload.VenueUploadId, stream);

            // Assert
            venueUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUpload.VenueUploadId }));

            using (new AssertionScope())
            {
                venueUpload.UploadStatus.Should().Be(UploadStatus.ProcessedSuccessfully);
                venueUpload.LastValidated.Should().Be(Clock.UtcNow);
                venueUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                venueUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessVenueFile_RowHasErrors_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(
                // Empty record will always yield errors
                new VenueRow());

            // Act
            await fileUploadProcessor.ProcessVenueFile(venueUpload.VenueUploadId, stream);

            // Assert
            venueUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUpload.VenueUploadId }));

            using (new AssertionScope())
            {
                venueUpload.UploadStatus.Should().Be(UploadStatus.ProcessedWithErrors);
                venueUpload.LastValidated.Should().Be(Clock.UtcNow);
                venueUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                venueUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ValidateAndUpsertVenueRows_InsertsRowsIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var row = new VenueRow()
            {
                ProviderVenueRef = "REF",
                VenueName = "Place",
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                Town = "Town",
                County = "County",
                Postcode = "AB1 2DE",
                Email = "place@provider.com",
                Telephone = "01234 567890",
                Website = "provider.com/place"
            };

            var uploadRows = new[] { row };

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(1);
                rows.Last().Should().BeEquivalentTo(new VenueUploadRow()
                {
                    RowNumber = 2,
                    LastUpdated = Clock.UtcNow,
                    LastValidated = Clock.UtcNow,
                    IsSupplementary = false,
                    VenueId = null,
                    AddressLine1 = row.AddressLine1,
                    AddressLine2 = row.AddressLine2,
                    County = row.County,
                    Email = row.Email,
                    Postcode = row.Postcode,
                    ProviderVenueRef = row.ProviderVenueRef,
                    Telephone = row.Telephone,
                    Town = row.Town,
                    VenueName = row.VenueName,
                    Website = row.Website
                }, config => config.Excluding(r => r.IsValid).Excluding(r => r.Errors));
            });
        }

        [Fact]
        public async Task ValidateAndUpsertVenueRows_FileIsMissingVenuesWithLiveOfferings_AddsSupplementaryRowsToFile()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Venue 1");
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venue.VenueId },
                createdBy: user);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var uploadRows = new[]
            {
                new VenueRow()
                {
                    VenueName = "Upload venue 1"
                }
            };

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(2);  // One from upload, one retained
                rows.Last().Should().BeEquivalentTo(new VenueUploadRow()
                {
                    RowNumber = 3,
                    LastUpdated = Clock.UtcNow,
                    LastValidated = Clock.UtcNow,
                    IsSupplementary = true,
                    VenueId = venue.VenueId,
                    AddressLine1 = venue.AddressLine1,
                    AddressLine2 = venue.AddressLine2,
                    County = venue.County,
                    Email = venue.Email,
                    Postcode = venue.Postcode,
                    ProviderVenueRef = venue.ProviderVenueRef,
                    Telephone = venue.Telephone,
                    Town = venue.Town,
                    VenueName = venue.VenueName,
                    Website = venue.Website
                }, config => config.Excluding(r => r.IsValid).Excluding(r => r.Errors));
            });
        }

        [Fact]
        public async Task ValidateAndUpsertVenueRows_NormalizesValidPostcode()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var row = new VenueRow()
            {
                Postcode = "ab12de",
            };

            var uploadRows = new[] { row };

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(1);
                rows.Last().Postcode.Should().Be("AB1 2DE");
            });
        }

        [Fact]
        public async Task ValidateAndUpsertVenueRows_DoesNotNormalizeInvalidPostcode()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var row = new VenueRow()
            {
                Postcode = "xxxx",
            };

            var uploadRows = new[] { row };

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(1);
                rows.Last().Postcode.Should().Be(row.Postcode);
            });
        }

        [Theory]
        [MemberData(nameof(GetInvalidRowsTestData))]
        public async Task ValidateAndUpsertVenueRows_RowsHasErrors_InsertsExpectedErrorCodesIntoDb(
            VenueRow row,
            IEnumerable<string> expectedErrorCodes,
            IEnumerable<VenueRow> additionalRows)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var uploadRows = new[] { row }.Concat(additionalRows ?? Enumerable.Empty<VenueRow>()).ToList();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Add a row into Postcodes table to ensure we don't have errors due to it missing
                // (ValidateAndUpsertVenueRows_PostcodeIsNotInDb_InsertsExpectedErrorCodesIntoDb tests that scenario)
                await AddPostcodeInfoForRows(dispatcher, uploadRows);

                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.First().IsValid.Should().BeFalse();
                rows.First().Errors.Should().BeEquivalentTo(expectedErrorCodes);
            });
        }

        [Fact]
        public async Task ValidateAndUpsertVenueRows_PostcodeIsNotInDb_InsertsExpectedErrorCodesIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var uploadRows = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 1).ToArray();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.First().IsValid.Should().BeFalse();
                rows.First().Errors.Should().BeEquivalentTo(new[] { "VENUE_POSTCODE_FORMAT" });
                rows.First().OutsideOfEngland.Should().BeNull();
            });
        }

        [Fact]
        public async Task ValidateAndUpsertVenueRows_PostcodeIsNotInEngland_InsertsExpectedOutsideOfEnglandValueIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var uploadRows = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 1).ToArray();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                await AddPostcodeInfoForRows(dispatcher, uploadRows, inEngland: false);

                // Act
                await fileUploadProcessor.ValidateAndUpsertVenueRows(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.First().IsValid.Should().BeTrue();
                rows.First().OutsideOfEngland.Should().BeTrue();
            });
        }

        private async Task UpdateStatusAndReleaseStatusCheck(
            TriggerableVenueUploadStatusUpdatesFileUploadProcessor uploadProcessor,
            Guid venueUploadId,
            UploadStatus uploadStatus)
        {
            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var updatedOn = Clock.UtcNow;

            if (uploadStatus == UploadStatus.Processing)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadProcessing()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingStartedOn = updatedOn
                }));
            }
            else if (uploadStatus == UploadStatus.ProcessedSuccessfully)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingCompletedOn = updatedOn,
                    IsValid = true
                }));
            }
            else if (uploadStatus == UploadStatus.ProcessedWithErrors)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingCompletedOn = updatedOn,
                    IsValid = false
                }));
            }
            else if (uploadStatus == UploadStatus.Abandoned)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadAbandoned()
                {
                    VenueUploadId = venueUploadId,
                    AbandonedOn = updatedOn
                }));
            }
            else if (uploadStatus == UploadStatus.Published)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadPublished()
                {
                    VenueUploadId = venueUploadId,
                    PublishedOn = updatedOn
                }));
            }
            else
            {
                throw new ArgumentException();
            }

            uploadProcessor.ReleaseUploadStatusCheck();
        }

        public static TheoryData<VenueRow, IEnumerable<string>, IEnumerable<VenueRow>> GetInvalidRowsTestData()
        {
            // Generic args correspond to:
            //   the row under test;
            //   the expected error codes for the row under test;
            //   any additional rows to create in the same upload (e.g. for testing duplicate validation)
            var result = new TheoryData<VenueRow, IEnumerable<string>, IEnumerable<VenueRow>>();

            // Name is missing
            AddSingleErrorTestCase(row => row.VenueName = null, "VENUE_NAME_REQUIRED");

            // Name is too long
            AddSingleErrorTestCase(row => row.VenueName = new string('x', 251), "VENUE_NAME_MAXLENGTH");

            // Name must be unique
            result.Add(
                CreateRow(row => row.VenueName = "Name"),
                new[] { "VENUE_NAME_UNIQUE" },
                new[] { CreateRow(row => row.VenueName = "NAME") });

            // Your venue reference is too long
            AddSingleErrorTestCase(row => row.ProviderVenueRef = new string('x', 256), "VENUE_PROVIDER_VENUE_REF_MAXLENGTH");

            // Your venue reference must be unique
            result.Add(
                CreateRow(row => row.ProviderVenueRef = "Ref"),
                new[] { "VENUE_PROVIDER_VENUE_REF_UNIQUE" },
                new[] { CreateRow(row => row.ProviderVenueRef = "REF") });

            // Address line 1 is missing
            AddSingleErrorTestCase(row => row.AddressLine1 = null, "VENUE_ADDRESS_LINE1_REQUIRED");

            // Address line 1 is too long
            AddSingleErrorTestCase(row => row.AddressLine1 = new string('x', 101), "VENUE_ADDRESS_LINE1_MAXLENGTH");

            // Address line 1 contains invalid characters
            AddSingleErrorTestCase(row => row.AddressLine1 = "%", "VENUE_ADDRESS_LINE1_FORMAT");

            // Address line 2 is too long
            AddSingleErrorTestCase(row => row.AddressLine2 = new string('x', 101), "VENUE_ADDRESS_LINE2_MAXLENGTH");

            // Address line 2 contains invalid characters
            AddSingleErrorTestCase(row => row.AddressLine2 = "%", "VENUE_ADDRESS_LINE2_FORMAT");

            // Town is missing
            AddSingleErrorTestCase(row => row.Town = null, "VENUE_TOWN_REQUIRED");

            // Town is too long
            AddSingleErrorTestCase(row => row.Town = new string('x', 76), "VENUE_TOWN_MAXLENGTH");

            // Town contains invalid characters
            AddSingleErrorTestCase(row => row.Town = "%", "VENUE_TOWN_FORMAT");

            // County is too long
            AddSingleErrorTestCase(row => row.County = new string('x', 76), "VENUE_COUNTY_MAXLENGTH");

            // County contains invalid characters
            AddSingleErrorTestCase(row => row.County = "%", "VENUE_COUNTY_FORMAT");

            // Postcode is missing
            AddSingleErrorTestCase(row => row.Postcode = "", "VENUE_POSTCODE_REQUIRED");

            // Postcode is not valid
            AddSingleErrorTestCase(row => row.Postcode = "x", "VENUE_POSTCODE_FORMAT");

            // Email is not valid
            AddSingleErrorTestCase(row => row.Email = "x", "VENUE_EMAIL_FORMAT");

            // Telephone is not valid
            AddSingleErrorTestCase(row => row.Telephone = "x", "VENUE_TELEPHONE_FORMAT");

            // Website is not valid
            AddSingleErrorTestCase(row => row.Website = "x", "VENUE_WEBSITE_FORMAT");

            return result;

            static VenueRow CreateRow(Action<VenueRow> configureRow)
            {
                var row = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 1).Single();
                configureRow(row);
                return row;
            }

            void AddSingleErrorTestCase(Action<VenueRow> configureRow, string errorCode) =>
                result.Add(
                    CreateRow(configureRow),
                    new[] { errorCode },
                    Enumerable.Empty<VenueRow>());
        }

        private static Task AddPostcodeInfoForRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IEnumerable<VenueRow> rows,
            bool inEngland = true)
        {
            return sqlQueryDispatcher.ExecuteQuery(new UpsertPostcodes()
            {
                Records = rows
                    .Select(r => r.Postcode)
                    .Where(r => Postcode.TryParse(r, out _))
                    .Distinct()
                    .Select(pc => new UpsertPostcodesRecord()
                    {
                        Postcode = pc,
                        Position = (1d, 1d),
                        InEngland = inEngland
                    })
            });
        }

        /// <summary>
        /// A version of <see cref="FileUploadProcessor"/> that overrides <see cref="GetPollingVenueUploadStatusUpdates(Guid)"/>
        /// to only query the database when it's triggered by <see cref="ReleaseUploadStatusCheck"/> instead of polling on a timer.
        /// </summary>
        private sealed class TriggerableVenueUploadStatusUpdatesFileUploadProcessor : FileUploadProcessor, IDisposable
        {
            // There's no un-typed Subject so we use Subject<object>. The values are never consumed.
            private readonly Subject<object> _subject;

            public TriggerableVenueUploadStatusUpdatesFileUploadProcessor(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory, IClock clock)
                : base(sqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), clock)
            {
                _subject = new Subject<object>();
            }

            public void ReleaseUploadStatusCheck() => _subject.OnNext(null);

            public void OnComplete() => _subject.OnCompleted();

            protected override IObservable<UploadStatus> GetPollingVenueUploadStatusUpdates(Guid venueUploadId) => _subject
                .SelectMany(_ => Observable.FromAsync(() => GetVenueUploadStatus(venueUploadId)));

            public void Dispose() => _subject.Dispose();
        }
    }
}
