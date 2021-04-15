using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public class VenueUploadProcessorTests
    {
        public VenueUploadProcessorTests()
        {
            SqlQueryDispatcher = new TestableSqlQueryDispatcher();

            var sqlQueryDispatcherFactory = new Mock<ISqlQueryDispatcherFactory>();
            sqlQueryDispatcherFactory
                .Setup(mock => mock.CreateDispatcher(It.IsAny<IsolationLevel>()))
                .Returns(SqlQueryDispatcher);

            var blobServiceClient = new Mock<BlobServiceClient>();

            Clock = new MutableClock();

            VenueUploadProcessor = new VenueUploadProcessor(
                sqlQueryDispatcherFactory.Object,
                blobServiceClient.Object,
                Clock,
                pollInterval: TimeSpan.Zero);
        }

        private MutableClock Clock { get; }

        private TestableSqlQueryDispatcher SqlQueryDispatcher { get; }

        private VenueUploadProcessor VenueUploadProcessor { get; }

        [Fact]
        public void GetUploadStatusUpdates_VenueUploadDoesNotExist_ReturnsArgumentException()
        {
            // Arrange
            var venueUploadId = Guid.NewGuid();

            SqlQueryDispatcher.SpecifyResult<GetVenueUpload, VenueUpload>(null);

            var statusUpdates = VenueUploadProcessor.GetUploadStatusUpdates(venueUploadId);

            var errored = new ManualResetEventSlim(false);
            Exception error = default;

            // Act
            using var subscription = statusUpdates.Subscribe(
                _ => { },
                onError: ex =>
                {
                    error = ex;
                    errored.Set();
                });

            // Assert
            errored.Wait(200);

            error.Should().BeOfType<ArgumentException>()
                .Subject.Message.Should().StartWith("Specified venue upload does not exist.");
        }

        [Fact]
        public async Task GetUploadStatusUpdates_EmitsInitialStatus()
        {
            // Arrange
            var venueUploadId = Guid.NewGuid();

            var queryExecutedCounter = new QueryExecutedCounter();

            var createdOn = Clock.UtcNow;

            SqlQueryDispatcher.SpecifyResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.Created,
                    CreatedOn = createdOn
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            var statusUpdates = VenueUploadProcessor.GetUploadStatusUpdates(venueUploadId)
                .SubscribeOn(NewThreadScheduler.Default);

            var results = new List<UploadStatus>();

            // Act
            using (var subscription = statusUpdates.Subscribe(v => results.Add(v)))
            {
                await queryExecutedCounter.WaitForExecutions(1);
            }

            // Assert
            results.First().Should().Be(UploadStatus.Created);
        }

        [Fact]
        public async Task GetUploadStatusUpdates_EmitsChangedStatus()
        {
            // Arrange
            var venueUploadId = Guid.NewGuid();

            var queryExecutedCounter = new QueryExecutedCounter();

            var createdOn = Clock.UtcNow;

            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var processingOn = Clock.UtcNow;

            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var completedOn = Clock.UtcNow;

            SqlQueryDispatcher.QueueResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.Created,
                    CreatedOn = createdOn
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            SqlQueryDispatcher.QueueResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.InProgress,
                    CreatedOn = createdOn,
                    ProcessingStartedOn = processingOn
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            SqlQueryDispatcher.QueueResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.Processed,
                    CreatedOn = createdOn,
                    ProcessingStartedOn = processingOn,
                    ProcessingCompletedOn = completedOn
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            var statusUpdates = VenueUploadProcessor.GetUploadStatusUpdates(venueUploadId)
                .SubscribeOn(NewThreadScheduler.Default);

            var results = new List<UploadStatus>();

            // Act
            using (var subscription = statusUpdates.Subscribe(v => results.Add(v)))
            {
                await queryExecutedCounter.WaitForExecutions(3);
            }

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created, UploadStatus.InProgress, UploadStatus.Processed });
        }

        [Fact]
        public async Task GetUploadStatusUpdates_DoesNotEmitDuplicateStatuses()
        {
            // Arrange
            var venueUploadId = Guid.NewGuid();

            var queryExecutedCounter = new QueryExecutedCounter();

            var createdOn = Clock.UtcNow;

            SqlQueryDispatcher.SpecifyResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.Created,
                    CreatedOn = createdOn
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            var statusUpdates = VenueUploadProcessor.GetUploadStatusUpdates(venueUploadId)
                .SubscribeOn(NewThreadScheduler.Default);

            var results = new List<UploadStatus>();

            // Act
            using (var subscription = statusUpdates.Subscribe(v => results.Add(v)))
            {
                await queryExecutedCounter.WaitForExecutions(3);
            }

            // Assert
            results.Should().BeEquivalentTo(new[] { UploadStatus.Created });
        }

        [Theory]
        [InlineData(UploadStatus.Abandoned)]
        [InlineData(UploadStatus.Published)]
        public void GetUploadStatusUpdates_CompletesWhenStatusIsTerminal(UploadStatus uploadStatus)
        {
            // Arrange
            var venueUploadId = Guid.NewGuid();

            var queryExecutedCounter = new QueryExecutedCounter();

            var createdOn = Clock.UtcNow;

            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var processingOn = Clock.UtcNow;

            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var completedOn = Clock.UtcNow;

            Clock.UtcNow += TimeSpan.FromMinutes(1);
            var terminatedOn = Clock.UtcNow;

            SqlQueryDispatcher.QueueResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.Processed,
                    CreatedOn = createdOn,
                    ProcessingStartedOn = processingOn,
                    ProcessingCompletedOn = completedOn
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            SqlQueryDispatcher.SpecifyResult<GetVenueUpload, VenueUpload>(
                new VenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    UploadStatus = UploadStatus.Processed,
                    CreatedOn = createdOn,
                    ProcessingStartedOn = processingOn,
                    ProcessingCompletedOn = completedOn,
                    AbandonedOn = uploadStatus == UploadStatus.Abandoned ? terminatedOn : (DateTime?)null,
                    PublishedOn = uploadStatus == UploadStatus.Published ? terminatedOn : (DateTime?)null
                },
                onResultEmitted: () => queryExecutedCounter.OnQueryExecuted());

            var statusUpdates = VenueUploadProcessor.GetUploadStatusUpdates(venueUploadId)
                .SubscribeOn(NewThreadScheduler.Default);

            var completed = new ManualResetEventSlim(false);

            // Act
            using var subscription = statusUpdates.Subscribe(_ => { }, onCompleted: () => completed.Set());

            // Assert
            completed.Wait(200);
        }

        private class QueryExecutedCounter
        {
            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
            private int _executions = 0;

            public void OnQueryExecuted()
            {
                _executions++;
                _autoResetEvent.Set();
            }

            public Task WaitForExecutions(int count, int millisecondsTimeout = 200) => Task.Run(() =>
            {
                while (_executions < count)
                {
                    _autoResetEvent.WaitOne(millisecondsTimeout);
                }
            });
        }
    }
}
