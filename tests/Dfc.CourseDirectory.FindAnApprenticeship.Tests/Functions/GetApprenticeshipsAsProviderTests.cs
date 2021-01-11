using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dfc.Providerportal.FindAnApprenticeship.Functions;
using Dfc.Providerportal.FindAnApprenticeship.Models;
using Dfc.Providerportal.FindAnApprenticeship.Storage;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dfc.ProviderPortal.FindAnApprenticeship.UnitTests.Functions
{
    public class GetApprenticeshipsAsProviderTests
    {
        private readonly Mock<IBlobStorageClient> _blobStorageClient;
        private readonly Mock<Func<DateTimeOffset>> _nowUtc;
        private readonly GetApprenticeshipsAsProvider _function;

        public GetApprenticeshipsAsProviderTests()
        {
            _blobStorageClient = new Mock<IBlobStorageClient>();
            _nowUtc = new Mock<Func<DateTimeOffset>>();
            _function = new GetApprenticeshipsAsProvider(_blobStorageClient.Object, _nowUtc.Object);
        }

        [Fact]
        public async Task Run_WithExportAvailableForToday_ReturnsTodaysExport()
        {
            var now = DateTimeOffset.UtcNow.Date;
            _nowUtc.Setup(s => s.Invoke())
                .Returns(now);

            var todaysBlob = "{\"id\":\"TodaysBlob\"}";
            var todaysExportKey = new ExportKey(now);

            var todaysBlobClient = new Mock<BlobClient>();
            todaysBlobClient.Setup(s => s.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ReturnBlob(todaysBlob));

            _blobStorageClient.Setup(s => s.GetBlobClient(It.Is<string>(blobName => blobName == todaysExportKey)))
                .Returns(todaysBlobClient.Object);

            var yesterdaysBlob = "{\"id\":\"YesterdaysBlob\"}";
            var yesterdayExportKey = new ExportKey(now.AddDays(-1));

            var yesterdaysBlobClient = new Mock<BlobClient>();
            yesterdaysBlobClient.Setup(s => s.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ReturnBlob(yesterdaysBlob));

            _blobStorageClient.Setup(s => s.GetBlobClient(It.Is<string>(blobName => blobName == yesterdayExportKey)))
                .Returns(yesterdaysBlobClient.Object);

            var result = await _function.Run(new Mock<HttpRequest>().Object, NullLogger.Instance, CancellationToken.None) as FileStreamResult;

            result.Should().NotBeNull();
            result.ContentType.Should().Be("application/json");

            using var sr = new StreamReader(result.FileStream);
            var contentResult = await sr.ReadToEndAsync();

            contentResult.Should().Be(todaysBlob);
        }

        [Fact]
        public async Task Run_WithNoExportAvailableForTodayAndExportAvailableForYesterday_ReturnsYesterdaysExport()
        {
            var now = DateTimeOffset.UtcNow;
            _nowUtc.Setup(s => s.Invoke())
                .Returns(now);

            var todaysExportKey = new ExportKey(now);

            var todaysBlobClient = new Mock<BlobClient>();
            todaysBlobClient.Setup(s => s.DownloadAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _blobStorageClient.Setup(s => s.GetBlobClient(It.Is<string>(blobName => blobName == todaysExportKey)))
                .Returns(todaysBlobClient.Object);

            var yesterdaysBlob = "{\"id\":\"YesterdaysBlob\"}";
            var yesterdayExportKey = new ExportKey(now.AddDays(-1));

            var yesterdaysBlobClient = new Mock<BlobClient>();
            yesterdaysBlobClient.Setup(s => s.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ReturnBlob(yesterdaysBlob));

            _blobStorageClient.Setup(s => s.GetBlobClient(It.Is<string>(blobName => blobName == yesterdayExportKey)))
                .Returns(yesterdaysBlobClient.Object);

            var result = await _function.Run(new Mock<HttpRequest>().Object, NullLogger.Instance, CancellationToken.None) as FileStreamResult;

            result.Should().NotBeNull();
            result.ContentType.Should().Be("application/json");

            using var sr = new StreamReader(result.FileStream);
            var contentResult = await sr.ReadToEndAsync();

            contentResult.Should().Be(yesterdaysBlob);
        }

        [Fact]
        public async Task Run_WithNoExportAvailableForTodayOrYesterday_ReturnsNotFound()
        {
            var now = DateTimeOffset.UtcNow;
            _nowUtc.Setup(s => s.Invoke())
                .Returns(now);

            var blobClient = new Mock<BlobClient>();
            blobClient.Setup(s => s.DownloadAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _blobStorageClient.Setup(s => s.GetBlobClient(It.IsAny<string>()))
                .Returns(blobClient.Object);

            var result = await _function.Run(new Mock<HttpRequest>().Object, NullLogger.Instance, CancellationToken.None) as NotFoundResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        private static Func<Response<BlobDownloadInfo>> ReturnBlob(string blobContent) => () =>
        {
            var response = new Mock<Response<BlobDownloadInfo>>();

            response.SetupGet(s => s.Value)
                .Returns(BlobsModelFactory.BlobDownloadInfo(content: new MemoryStream(Encoding.UTF8.GetBytes(blobContent))));

            return response.Object;
        };
    }
}