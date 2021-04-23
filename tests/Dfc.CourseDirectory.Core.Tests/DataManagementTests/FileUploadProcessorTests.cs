using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;
using static Dfc.CourseDirectory.Core.DataManagement.FileUploadProcessor;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public class FileUploadProcessorTests : DatabaseTestBase
    {
        public FileUploadProcessorTests(DatabaseTestBaseFixture fixture) : base(fixture)
        {
        }

        public static TheoryData<byte[], bool> LooksLikeCsvData { get; } = new TheoryData<byte[], bool>()
        {
            { Encoding.UTF8.GetBytes("first,second,third\n1,2,3"), true },
            { Encoding.UTF8.GetBytes("abc\n"), false },

            // This data is a small PNG file
            { Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABcAAAAbCAIAAAAYioOMAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAEkSURBVEhLY/hPDTBqCnYwgkz5tf/ge0sHIPqxai1UCDfAacp7Q8u3MipA9E5ZGyoEA7+vXPva0IJsB05TgJohpgARVOj//++LlgF1wsWBCGIHTlO+TZkBVwoV6Z0IF0FGQCkUU36fPf8pJgkeEMjqcBkBVA+URZjy99UruC+ADgGKwJV+a++GsyEIGGpfK2t/HTsB0YswBRhgcEUQ38K5yAhrrCFMgUcKBAGdhswFIjyxjjAFTc87LSMUrrL2n9t3oUoxAE5T0BAkpHABqCmY7kdGn5MzIcpwAagpyEGLiSBq8AAGzOQIQT937IKzoWpxAwa4UmQESUtwLkQpHgA1BS0VQQBppgBt/vfjB1QACZBmClYjgIA0UwgiqFrcgBqm/P8PAGN09WCiWJ70AAAAAElFTkSuQmCC"), false },
        };

        [Theory]
        [InlineData("", true)]
        [InlineData("77u/", true)]  // UTF-8 BOM
        [InlineData("Zmlyc3Qsc2Vjb25kLHRoaXJk", false)]  // "first,second,third"
        public async Task FileIsEmpty_ReturnsExpectedResult(string base64Content, bool expectedResult)
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var stream = new MemoryStream(Convert.FromBase64String(base64Content));
            stream.Seek(0L, SeekOrigin.Begin);

            // Act
            var result = await fileUploadProcessor.FileIsEmpty(stream);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(LooksLikeCsvData))]
        public async Task LooksLikeCsv_ReturnsExpectedResult(byte[] content, bool expectedResult)
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var stream = new MemoryStream(content);
            stream.Seek(0L, SeekOrigin.Begin);

            // Act
            var result = await fileUploadProcessor.LooksLikeCsv(stream);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task FileMatchesSchema_HeaderHasMissingColumn_ReturnsInvalidHeaderResult()
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(csvWriter =>
            {
                // Miss out VENUE_NAME, POSTCODE
                csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                csvWriter.WriteField("ADDRESS_LINE_1");
                csvWriter.WriteField("ADDRESS_LINE_2");
                csvWriter.WriteField("TOWN_OR_CITY");
                csvWriter.WriteField("COUNTY");
                csvWriter.WriteField("EMAIL");
                csvWriter.WriteField("PHONE");
                csvWriter.WriteField("WEBSITE");
                csvWriter.NextRecord();
            },
            writeHeader: false);

            // Act
            var (result, missingHeaders) = await fileUploadProcessor.FileMatchesSchema<VenueRow>(stream);

            // Assert
            result.Should().Be(FileMatchesSchemaResult.InvalidHeader);
            missingHeaders.Should().BeEquivalentTo(new[]
            {
                "VENUE_NAME",
                "POSTCODE"
            });
        }

        [Theory]
        [InlineData(1)]  // Less than valid row
        //[InlineData(99]  // More than valid row - we don't have a way of checking this currently
        public async Task FileMatchesSchema_RowHasIncorrectColumnCount_ReturnsInvalidRows(int columnCount)
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, Mock.Of<BlobServiceClient>(), Clock);

            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(csvWriter =>
            {
                for (int i = 0; i < columnCount; i++)
                {
                    csvWriter.WriteField("value");
                }

                csvWriter.NextRecord();
            });

            // Act
            var (result, missingHeaders) = await fileUploadProcessor.FileMatchesSchema<VenueRow>(stream);

            // Assert
            result.Should().Be(FileMatchesSchemaResult.InvalidRows);
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
        public async Task ProcessFile_InsertsRowsInDbAndSetStatusToProcessed()
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
