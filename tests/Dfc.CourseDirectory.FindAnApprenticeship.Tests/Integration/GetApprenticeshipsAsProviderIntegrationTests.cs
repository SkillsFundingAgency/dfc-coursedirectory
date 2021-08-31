using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.FindAnApprenticeship.Tests.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Functions;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Storage;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Provider = Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Providers.Provider;

namespace Dfc.CourseDirectory.FindAnApprenticeship.Tests.Integration
{
    public class GetApprenticeshipsAsProviderIntegrationTests
    {
        private readonly Mock<Func<DateTimeOffset>> _nowUtc;
        private readonly Mock<IBlobStorageClient> _blobStorageClient;

        private readonly Mock<IProviderService> _providerService;
        private readonly IProviderServiceClient _providerServiceClient;
        private readonly IDASHelper _DASHelper;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly Mock<ICosmosDbQueryDispatcher> _cosmosDbQueryDispatcher;
        private readonly Mock<ISqlQueryDispatcher> _sqlQueryDispatcher;

        private readonly GenerateProviderExportFunction _generateProviderExportFunction;
        private readonly GetApprenticeshipsAsProvider _getApprenticeshipAsProviderFunction;

        public GetApprenticeshipsAsProviderIntegrationTests()
        {
            _nowUtc = new Mock<Func<DateTimeOffset>>();
            _blobStorageClient = new Mock<IBlobStorageClient>();

            _providerService = new Mock<IProviderService>();
            _providerServiceClient = new ProviderServiceClient(_providerService.Object);
            _cosmosDbQueryDispatcher = new Mock<ICosmosDbQueryDispatcher>();
            _sqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();

            var telemetryClient = MockTelemetryHelper.Initialize();
            _DASHelper = new DASHelper(telemetryClient);
            _apprenticeshipService = new ApprenticeshipService(_DASHelper, _providerServiceClient, telemetryClient, _sqlQueryDispatcher.Object, _cosmosDbQueryDispatcher.Object);

            _generateProviderExportFunction = new GenerateProviderExportFunction(_apprenticeshipService, _blobStorageClient.Object);
            _getApprenticeshipAsProviderFunction = new GetApprenticeshipsAsProvider(_blobStorageClient.Object, _nowUtc.Object);
        }

        [Fact(Skip = "Integration test data needs replacing with SQL equivalents")]
        public async Task Run_ReturnsExpectedResult()
        {
            var now = DateTimeOffset.Now;

            _nowUtc.Setup(s => s.Invoke())
                .Returns(now);

            _providerService.Setup(s => s.GetActiveProvidersAsync())
                .Returns(async () => JsonConvert.DeserializeObject<IEnumerable<Provider>>(await File.ReadAllTextAsync("Integration/providers.json")));

            _cosmosDbQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetFeChoicesByProviderUkprns>()))
                .Returns<GetFeChoicesByProviderUkprns>(async r =>
                    JsonConvert.DeserializeObject<IEnumerable<Core.DataStore.CosmosDb.Models.FeChoice>>(await File.ReadAllTextAsync("Integration/fechoices.json"))
                        .Where(f => r.ProviderUkprns.Contains(f.UKPRN)).ToDictionary(f => f.UKPRN, f => f));

            _sqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetAllApprenticeships>()))
                .ReturnsAsync(() => JsonConvert.DeserializeObject<List<Apprenticeship>>(File.ReadAllText("Integration/apprenticeships.json")));

            var blobClient = new Mock<BlobClient>();
            var blobLeaseClient = new Mock<BlobLeaseClient>();
            var blobBytes = default(byte[]);

            blobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new MockResponse<bool>(true));

            blobLeaseClient.Setup(s => s.AcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new MockResponse<BlobLease>(BlobsModelFactory.BlobLease(new ETag(Guid.NewGuid().ToString()), DateTimeOffset.UtcNow, Guid.NewGuid().ToString())));

            _blobStorageClient.Setup(s => s.GetBlobClient(It.Is<string>(blobName => blobName == new ExportKey(now))))
                .Returns(blobClient.Object);

            _blobStorageClient.Setup(s => s.GetBlobLeaseClient(blobClient.Object, It.IsAny<string>()))
                .Returns(blobLeaseClient.Object);

            blobClient.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobRequestConditions>(), It.IsAny<IProgress<long>>(), It.IsAny<AccessTier?>(), It.IsAny<StorageTransferOptions>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, BlobHttpHeaders, IDictionary<string, string>, BlobRequestConditions, IProgress<long>, AccessTier?, StorageTransferOptions, CancellationToken>((s, h, m, c, p, a, t, ct) =>
                {
                    using (var ms = new MemoryStream())
                    {
                        s.CopyTo(ms);
                        blobBytes = ms.ToArray();
                    }
                });

            blobClient.Setup(s => s.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new MockResponse<BlobDownloadInfo>(BlobsModelFactory.BlobDownloadInfo(content: new MemoryStream(blobBytes))));

            await _generateProviderExportFunction.Run(new TimerInfo(new ScheduleStub(), new ScheduleStatus()), NullLogger.Instance, CancellationToken.None);

            var result = await _getApprenticeshipAsProviderFunction.Run(new Mock<HttpRequest>().Object, NullLogger.Instance, CancellationToken.None);

            var contentResult = result.Should().BeOfType<FileStreamResult>().Subject;
            contentResult.Should().NotBeNull();
            contentResult.ContentType.Should().Be("application/json");

            using var sr = new StreamReader(contentResult.FileStream);
            var resultJToken = JToken.Parse(await sr.ReadToEndAsync());
            var expectedResultJToken = JToken.Parse(await File.ReadAllTextAsync("Integration/expectedresults.json"));

            var resultIsExpected = JToken.DeepEquals(resultJToken, expectedResultJToken);

            if (!resultIsExpected)
            {
                // Output the results so we can investigate further
                await File.WriteAllTextAsync("Integration/results.json", JsonConvert.SerializeObject(resultJToken, Formatting.Indented));
            }

            resultIsExpected.Should().BeTrue();
        }

        private class MockResponse<T> : Response<T>
        {
            public override T Value { get; }

            public MockResponse(T value)
            {
                Value = value;
            }

            public override Response GetRawResponse()
            {
                throw new NotImplementedException();
            }
        }

        private class ScheduleStub : TimerSchedule
        {
            public override DateTime GetNextOccurrence(DateTime now)
            {
                throw new NotImplementedException();
            }
        }
    }
}
