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
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
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
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var venueUploadId = Guid.NewGuid();

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdatesForProvider(venueUploadId);

            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => { }, cts.Token);
            uploadProcessor.OnComplete();
            var error = await Record.ExceptionAsync(() => completed);

            error.Should().BeOfType<InvalidStateException>().Subject.Reason.Should().Be(InvalidStateReason.NoUnpublishedVenueUpload);
        }

        [Fact]
        public async Task GetVenueUploadStatusUpdates_EmitsInitialStatus()
        {
            // Arrange
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdatesForProvider(provider.ProviderId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            await uploadProcessor.ReleaseUploadStatusCheck();
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.First().Should().Be(UploadStatus.Created);
        }

        [Fact]
        public async Task GetVenueUploadStatusUpdates_EmitsChangedStatus()
        {
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdatesForProvider(provider.ProviderId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            await uploadProcessor.ReleaseUploadStatusCheck();  // Created
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, UploadStatus.Processing, user);
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, UploadStatus.ProcessedSuccessfully, user);
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created, UploadStatus.Processing, UploadStatus.ProcessedSuccessfully });
        }

        [Fact]
        public async Task GetVenueUploadStatusUpdates_DoesNotEmitDuplicateStatuses()
        {
            // Arrange
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdatesForProvider(provider.ProviderId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            await uploadProcessor.ReleaseUploadStatusCheck();
            await uploadProcessor.ReleaseUploadStatusCheck();
            await uploadProcessor.ReleaseUploadStatusCheck();
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
            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var statusUpdates = uploadProcessor.GetVenueUploadStatusUpdatesForProvider(provider.ProviderId);

            var results = new List<UploadStatus>();
            using var cts = new CancellationTokenSource();

            // Act
            cts.CancelAfter(500);
            var completed = statusUpdates.ForEachAsync(v => results.Add(v), cts.Token);
            await uploadProcessor.ReleaseUploadStatusCheck();  // Created
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, uploadStatus, user);
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

            var rows = new[] { new CsvVenueRow() { ProviderVenueRef = "REF", VenueName = "NAME" } }.ToDataUploadRowCollection();

            var existingVenues = new[] { venue1 }.Select(v => v.Adapt<VenueMatchInfo>());

            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

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

            var rows = new[] { new CsvVenueRow() { VenueName = "NAME" } }.ToDataUploadRowCollection();

            var existingVenues = new[] { venue1 }.Select(v => v.Adapt<VenueMatchInfo>());

            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

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
                new CsvVenueRow() { VenueName = "NAME2", ProviderVenueRef = "REF" },
                new CsvVenueRow() { VenueName = "NAME" },
            }.ToDataUploadRowCollection();

            var existingVenues = new[] { venue1 }.Select(v => v.Adapt<VenueMatchInfo>());

            var uploadProcessor = new TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                SqlQueryDispatcherFactory,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

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
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var uploadRows = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 3).ToArray();
            await WithSqlQueryDispatcher(dispatcher => AddPostcodeInfoForRows(dispatcher, uploadRows.ToDataUploadRowCollection()));
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
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(
                // Empty record will always yield errors
                new CsvVenueRow());

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
        public async Task PublishVenueUpload_StatusIsProcessedWithErrors_ReturnsUploadHasErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedWithErrors);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            // Act
            var result = await fileUploadProcessor.PublishVenueUploadForProvider(provider.ProviderId, user);

            // Assert
            result.Status.Should().Be(PublishResultStatus.UploadHasErrors);
        }

        [Fact]
        public async Task PublishVenueUpload_StatusIsProcessedWithErrorsAfterRevalidation_ReturnsUploadHasErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var oldVenue1 = await TestData.CreateVenue(
                provider.ProviderId,
                createdBy: user,
                venueName: "Old venue name",
                providerVenueRef: "VENUE1");

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = "Venue";
                        record.ProviderVenueRef = "VENUE1";
                        record.VenueId = oldVenue1.VenueId;
                    });
                });

            // Ensure we have a record in the Postcodes table for extracting lat/lng
            var postcodePosition = (Latitude: 1d, Longitude: 2d);
            foreach (var postcode in venueUploadRows.Select(r => r.Postcode).Distinct())
            {
                await TestData.CreatePostcodeInfo(postcode, postcodePosition.Latitude, postcodePosition.Longitude);
            }

            // Add a new venue for the provider and link a T-Level to it (so the venue cannot be removed by the publish).
            // Induce an error by having this venue's name clash with the name of a row in the upload.

            Clock.UtcNow += TimeSpan.FromDays(1);

            var oldVenue2 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Venue", providerVenueRef: "VENUE2");

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinitions.First().TLevelDefinitionId,
                new[] { oldVenue2.VenueId },
                createdBy: user);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            // Act
            var result = await fileUploadProcessor.PublishVenueUploadForProvider(provider.ProviderId, user);

            // Assert
            result.Status.Should().Be(PublishResultStatus.UploadHasErrors);
        }

        [Fact]
        public async Task PublishVenueUpload_RevalidationAddsSupplementaryRows_PublishesSuccessfully()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = "Venue1";
                        record.ProviderVenueRef = "VENUE1";
                    });
                });

            // Ensure we have a record in the Postcodes table for extracting lat/lng
            var postcodePosition = (Latitude: 1d, Longitude: 2d);
            foreach (var postcode in venueUploadRows.Select(r => r.Postcode).Distinct())
            {
                await TestData.CreatePostcodeInfo(postcode, postcodePosition.Latitude, postcodePosition.Longitude);
            }

            // Add a new venue for the provider and link a T-Level to it (so the venue cannot be removed by the publish).
            // Revalidation should kick in and add a supplementary row (without any validation errors).

            Clock.UtcNow += TimeSpan.FromDays(1);

            var oldVenue2 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Venue2", providerVenueRef: "VENUE2");

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinitions.First().TLevelDefinitionId,
                new[] { oldVenue2.VenueId },
                createdBy: user);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            // Act
            var result = await fileUploadProcessor.PublishVenueUploadForProvider(provider.ProviderId, user);

            // Assert
            result.Status.Should().Be(PublishResultStatus.Success);
            result.PublishedCount.Should().Be(2);
        }

        [Fact]
        public async Task PublishVenueUpload_CanBePublished_UpsertsRowsArchivesUnmatchedVenuesAndSetsStatusToPublished()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var oldVenue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Venue 1", providerVenueRef: "VENUE1");
            var oldVenue2 = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Venue 2");

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    // Add two rows; one matching `oldVenue` and one that doesn't match an existing venue

                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = "Venue 1";
                        record.ProviderVenueRef = "VENUE1";
                        record.VenueId = oldVenue1.VenueId;
                    });

                    rowBuilder.AddRow(record => record.VenueName = "New venue 1");
                });

            // Ensure we have a record in the Postcodes table for extracting lat/lng
            var postcodePosition = (Latitude: 1d, Longitude: 2d);
            foreach (var postcode in venueUploadRows.Select(r => r.Postcode).Distinct())
            {
                await TestData.CreatePostcodeInfo(postcode, postcodePosition.Latitude, postcodePosition.Longitude);
            }

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            // Act
            var result = await fileUploadProcessor.PublishVenueUploadForProvider(provider.ProviderId, user);

            // Assert
            result.Status.Should().Be(PublishResultStatus.Success);
            result.PublishedCount.Should().Be(2);

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                venueUpload = await dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUpload.VenueUploadId });
                venueUpload.UploadStatus.Should().Be(UploadStatus.Published);
                venueUpload.PublishedOn.Should().Be(Clock.UtcNow);

                var providerVenues = await dispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = provider.ProviderId });

                providerVenues.Should().BeEquivalentTo(new[]
                {
                    new Venue()
                    {
                        VenueId = oldVenue1.VenueId,
                        VenueName = venueUploadRows[0].VenueName,
                        ProviderId = provider.ProviderId,
                        ProviderVenueRef = venueUploadRows[0].ProviderVenueRef,
                        AddressLine1 = venueUploadRows[0].AddressLine1,
                        AddressLine2 = venueUploadRows[0].AddressLine2,
                        Town = venueUploadRows[0].Town,
                        County = venueUploadRows[0].County,
                        Postcode = venueUploadRows[0].Postcode,
                        Latitude = postcodePosition.Latitude,
                        Longitude = postcodePosition.Longitude,
                        Email = venueUploadRows[0].Email,
                        Telephone = venueUploadRows[0].Telephone,
                        Website = venueUploadRows[0].Website
                    },
                    new Venue()
                    {
                        VenueId = venueUploadRows[1].VenueId,
                        VenueName = venueUploadRows[1].VenueName,
                        ProviderId = provider.ProviderId,
                        ProviderVenueRef = venueUploadRows[1].ProviderVenueRef,
                        AddressLine1 = venueUploadRows[1].AddressLine1,
                        AddressLine2 = venueUploadRows[1].AddressLine2,
                        Town = venueUploadRows[1].Town,
                        County = venueUploadRows[1].County,
                        Postcode = venueUploadRows[1].Postcode,
                        Latitude = postcodePosition.Latitude,
                        Longitude = postcodePosition.Longitude,
                        Email = venueUploadRows[1].Email,
                        Telephone = venueUploadRows[1].Telephone,
                        Website = venueUploadRows[1].Website
                    },
                });
            });
        }

        [Fact]
        public async Task ValidateVenueUploadFile_InsertsRowsIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var row = new CsvVenueRow()
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

            var uploadRows = new[] { row }.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(1);
                rows.Last().Should().BeEquivalentTo(new VenueUploadRow()
                {
                    RowNumber = 2,
                    LastUpdated = Clock.UtcNow,
                    LastValidated = Clock.UtcNow,
                    IsSupplementary = false,
                    IsDeletable = true,
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
                }, config => config.Excluding(r => r.IsValid).Excluding(r => r.Errors).Excluding(r => r.VenueId));
            });
        }

        [Fact]
        public async Task ValidateVenueUploadFile_FileIsMissingVenuesWithLiveOfferings_AddsSupplementaryRowsToFile()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Venue 1");
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venue.VenueId },
                createdBy: user);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var uploadRows = new[]
            {
                new CsvVenueRow()
                {
                    VenueName = "Upload venue 1"
                }
            }.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(2);  // One from upload, one retained
                rows.Last().Should().BeEquivalentTo(new VenueUploadRow()
                {
                    RowNumber = 3,
                    LastUpdated = Clock.UtcNow,
                    LastValidated = Clock.UtcNow,
                    IsSupplementary = true,
                    VenueId = venue.VenueId,
                    IsDeletable = false,
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
        public async Task ValidateVenueUploadFile_NormalizesValidPostcode()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var row = new CsvVenueRow()
            {
                Postcode = "ab12de",
            };

            var uploadRows = new[] { row }.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(1);
                rows.Last().Postcode.Should().Be("AB1 2DE");
            });
        }

        [Fact]
        public async Task ValidateVenueUploadFile_DoesNotNormalizeInvalidPostcode()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var row = new CsvVenueRow()
            {
                Postcode = "xxxx",
            };

            var uploadRows = new[] { row }.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.Count.Should().Be(1);
                rows.Last().Postcode.Should().Be(row.Postcode);
            });
        }

        [Theory]
        [MemberData(nameof(GetInvalidRowsTestData))]
        public async Task ValidateVenueUploadFile_RowsHasErrors_InsertsExpectedErrorCodesIntoDb(
            CsvVenueRow row,
            IEnumerable<string> expectedErrorCodes,
            IEnumerable<CsvVenueRow> additionalRows)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var uploadRows = new[] { row }.Concat(additionalRows ?? Enumerable.Empty<CsvVenueRow>()).ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Add a row into Postcodes table to ensure we don't have errors due to it missing
                // (ValidateVenueUploadFile_PostcodeIsNotInDb_InsertsExpectedErrorCodesIntoDb tests that scenario)
                await AddPostcodeInfoForRows(dispatcher, uploadRows);

                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.First().IsValid.Should().BeFalse();
                rows.First().Errors.Should().BeEquivalentTo(expectedErrorCodes);
            });
        }

        [Fact]
        public async Task ValidateVenueUploadFile_PostcodeIsNotInDb_InsertsExpectedErrorCodesIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var uploadRows = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 1).ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.First().IsValid.Should().BeFalse();
                rows.First().Errors.Should().BeEquivalentTo(new[] { "VENUE_POSTCODE_FORMAT" });
                rows.First().OutsideOfEngland.Should().BeNull();
            });
        }

        [Fact]
        public async Task ValidateVenueUploadFile_PostcodeIsNotInEngland_InsertsExpectedOutsideOfEnglandValueIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var uploadRows = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 1).ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                await AddPostcodeInfoForRows(dispatcher, uploadRows, inEngland: false);

                // Act
                await fileUploadProcessor.ValidateVenueUploadFile(
                    dispatcher,
                    venueUpload.VenueUploadId,
                    venueUpload.ProviderId,
                    uploadRows);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

                rows.First().IsValid.Should().BeTrue();
                rows.First().OutsideOfEngland.Should().BeTrue();
            });
        }

        private async Task UpdateStatusAndReleaseStatusCheck(
            TriggerableVenueUploadStatusUpdatesFileUploadProcessor uploadProcessor,
            Guid venueUploadId,
            UploadStatus uploadStatus,
            UserInfo user)
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
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new PublishVenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    PublishedBy = user,
                    PublishedOn = updatedOn
                }));
            }
            else
            {
                throw new ArgumentException();
            }

            await uploadProcessor.ReleaseUploadStatusCheck();
        }

        public static TheoryData<CsvVenueRow, IEnumerable<string>, IEnumerable<CsvVenueRow>> GetInvalidRowsTestData()
        {
            // Generic args correspond to:
            //   the row under test;
            //   the expected error codes for the row under test;
            //   any additional rows to create in the same upload (e.g. for testing duplicate validation)
            var result = new TheoryData<CsvVenueRow, IEnumerable<string>, IEnumerable<CsvVenueRow>>();

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

            static CsvVenueRow CreateRow(Action<CsvVenueRow> configureRow)
            {
                var row = DataManagementFileHelper.CreateVenueUploadRows(rowCount: 1).Single();
                configureRow(row);
                return row;
            }

            void AddSingleErrorTestCase(Action<CsvVenueRow> configureRow, string errorCode) =>
                result.Add(
                    CreateRow(configureRow),
                    new[] { errorCode },
                    Enumerable.Empty<CsvVenueRow>());
        }

        private static Task AddPostcodeInfoForRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            VenueDataUploadRowInfoCollection rows,
            bool inEngland = true)
        {
            return sqlQueryDispatcher.ExecuteQuery(new UpsertPostcodes()
            {
                Records = rows
                    .Select(r => r.Data.Postcode)
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
        /// A version of <see cref="FileUploadProcessor"/> that overrides <see cref="GetVenueUploadStatusUpdates(Guid)"/>
        /// to only query the database when it's triggered by <see cref="ReleaseUploadStatusCheck"/> instead of polling on a timer.
        /// </summary>
        private sealed class TriggerableVenueUploadStatusUpdatesFileUploadProcessor : FileUploadProcessor, IDisposable
        {
            private readonly ReplaySubject<UploadStatus> _subject;

            private readonly TaskCompletionSource<Guid> _venueUploadIdTcs;

            public TriggerableVenueUploadStatusUpdatesFileUploadProcessor(
                ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
                IClock clock,
                IRegionCache regionCache)
                : base(sqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), clock, regionCache)
            {
                _subject = new ReplaySubject<UploadStatus>();
                _venueUploadIdTcs = new TaskCompletionSource<Guid>();
            }

            public async Task ReleaseUploadStatusCheck()
            {
                var venueUploadId = await _venueUploadIdTcs.Task;

                try
                {
                    _subject.OnNext(await GetVenueUploadStatus(venueUploadId));
                }
                catch (Exception ex)
                {
                    _subject.OnError(ex);
                }
            }

            public void OnComplete() => _subject.OnCompleted();

            public void Dispose() => _subject.Dispose();

            protected override IObservable<UploadStatus> GetVenueUploadStatusUpdates(Guid venueUploadId)
            {
                _venueUploadIdTcs.SetResult(venueUploadId);

                return _subject;
            }
        }
    }
}
