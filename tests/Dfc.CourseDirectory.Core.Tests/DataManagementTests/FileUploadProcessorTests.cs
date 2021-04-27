using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

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

            var stream = CreateVenueUploadCsvStream(recordCount: 3);

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

        private Stream CreateVenueUploadCsvStream(int recordCount)
        {
            // N.B. We deliberately do not use the VenueRow class here to ensure we notice if any columns change name

            var stream = new MemoryStream();

            using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                csvWriter.WriteField("VENUE_NAME");
                csvWriter.WriteField("ADDRESS_LINE_1");
                csvWriter.WriteField("ADDRESS_LINE_2");
                csvWriter.WriteField("TOWN_OR_CITY");
                csvWriter.WriteField("COUNTY");
                csvWriter.WriteField("POSTCODE");
                csvWriter.WriteField("EMAIL");
                csvWriter.WriteField("PHONE");
                csvWriter.WriteField("WEBSITE");
                csvWriter.NextRecord();

                var venueNames = new HashSet<string>();

                for (int i = 0; i < recordCount; i++)
                {
                    // Venue names have to be unique
                    string venueName;
                    do
                    {
                        venueName = Faker.Company.Name();
                    }
                    while (!venueNames.Add(venueName));

                    csvWriter.WriteField(Guid.NewGuid().ToString());
                    csvWriter.WriteField(venueName);
                    csvWriter.WriteField(Faker.Address.StreetAddress());
                    csvWriter.WriteField(Faker.Address.SecondaryAddress());
                    csvWriter.WriteField(Faker.Address.City());
                    csvWriter.WriteField(Faker.Address.UkCounty());
                    csvWriter.WriteField(Faker.Address.UkPostCode());
                    csvWriter.WriteField(Faker.Internet.Email());
                    csvWriter.WriteField(string.Empty); // There's no Faker method for a UK phone number
                    csvWriter.WriteField(Faker.Internet.Url());

                    csvWriter.NextRecord();
                }
            }

            stream.Seek(0L, SeekOrigin.Begin);

            return stream;
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
