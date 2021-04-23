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
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, UploadStatus.InProgress);
            await UpdateStatusAndReleaseStatusCheck(uploadProcessor, venueUpload.VenueUploadId, UploadStatus.Processed);
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created, UploadStatus.InProgress, UploadStatus.Processed });
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
        public async Task ValidateAndUpsertVenueRows_InsertsRowsIntoDb()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.InProgress);

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
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: user, UploadStatus.InProgress);

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
                    IsValid = true,
                    Errors = new string[0],
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
                });
            });
        }

        [Fact]
        public async Task ProcessVenueFile_SetStatusToProcessed()
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venueUpload = await TestData.CreateVenueUpload(provider.ProviderId, user, UploadStatus.Created);

            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(recordCount: 3);

            // Act
            await fileUploadProcessor.ProcessVenueFile(venueUpload.VenueUploadId, stream);

            // Assert
            venueUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUpload.VenueUploadId }));

            using (new AssertionScope())
            {
                venueUpload.UploadStatus.Should().Be(UploadStatus.Processed);
                venueUpload.IsValid.Should().NotBeNull();
                venueUpload.LastValidated.Should().Be(Clock.UtcNow);
                venueUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                venueUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        private async Task UpdateStatusAndReleaseStatusCheck(
            TriggerableVenueUploadStatusUpdatesFileUploadProcessor uploadProcessor,
            Guid venueUploadId,
            UploadStatus uploadStatus)
        {
            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var updatedOn = Clock.UtcNow;

            if (uploadStatus == UploadStatus.InProgress)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadInProgress()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingStartedOn = updatedOn
                }));
            }
            else if (uploadStatus == UploadStatus.Processed)
            {
                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingCompletedOn = updatedOn,
                    IsValid = true
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
