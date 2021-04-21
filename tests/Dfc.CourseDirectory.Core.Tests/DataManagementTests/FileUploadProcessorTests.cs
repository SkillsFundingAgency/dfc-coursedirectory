using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public class FileUploadProcessorTests : DatabaseTestBase
    {
        public FileUploadProcessorTests(DatabaseTestBaseFixture fixture) : base(fixture)
        {
        }

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
            await UpdateStatusAndReleaseStatusCheck(UploadStatus.InProgress);
            await UpdateStatusAndReleaseStatusCheck(UploadStatus.Processed);
            uploadProcessor.OnComplete();
            await completed;

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created, UploadStatus.InProgress, UploadStatus.Processed });

            async Task UpdateStatusAndReleaseStatusCheck(UploadStatus uploadStatus)
            {
                Clock.UtcNow += TimeSpan.FromMinutes(1);
                var updatedOn = Clock.UtcNow;

                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpdateVenueUploadStatus()
                {
                    VenueUploadId = venueUpload.VenueUploadId,
                    UploadStatus = uploadStatus,
                    ChangedOn = updatedOn
                }));

                uploadProcessor.ReleaseUploadStatusCheck();
            }
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
            await UpdateStatusAndReleaseStatusCheck(uploadStatus);
            uploadProcessor.OnComplete();
            await completed;

            async Task UpdateStatusAndReleaseStatusCheck(UploadStatus uploadStatus)
            {
                Clock.UtcNow += TimeSpan.FromMinutes(1);
                var updatedOn = Clock.UtcNow;

                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpdateVenueUploadStatus()
                {
                    VenueUploadId = venueUpload.VenueUploadId,
                    UploadStatus = uploadStatus,
                    ChangedOn = updatedOn
                }));

                uploadProcessor.ReleaseUploadStatusCheck();
            }
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
