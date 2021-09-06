using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
{
    public class DownloadErrorsTests : MvcTestBase
    {
        public DownloadErrorsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// TODO: Stop published status from skipping when PublishApprenticeshipUploadHandler is wired up.
        /// </summary>
        /// <param name="uploadStatus"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        //[InlineData(UploadStatus.Published, Skip = "Skip is ignored under PublishApprenticeshipUploadHandler is wired up")]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_ProviderHasNoApprenticeshipUploadAtProcessedWithErrorsStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            if (uploadStatus != null)
            {
                await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/download-errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsRowsWithErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider", providerType: ProviderType.Apprenticeships);
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 2, standardName: "Test Standard");
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            var (apprenticeshipUpload, apprenticeshipUploadRows) = await TestData.CreateApprenticeshipUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(standard.StandardCode, standard.Version, record =>
                    {
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["APPRENTICESHIP_TELEPHONE_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["APPRENTICESHIP_INFORMATION_REQUIRED"].ErrorCode
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/download-errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_apprenticeships_errors_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.HeaderRecord.Should().BeEquivalentTo(new[]
            {
                "STANDARD_CODE",
                "STANDARD_VERSION",
                "APPRENTICESHIP_INFORMATION",
                "APPRENTICESHIP_WEBPAGE",
                "CONTACT_EMAIL",
                "CONTACT_PHONE",
                "CONTACT_URL",
                "DELIVERY_METHOD",
                "VENUE",
                "YOUR_VENUE_REFERENCE",
                "RADIUS",
                "DELIVERY_MODE",
                "NATIONAL_DELIVERY",
                "SUB_REGION",
                "ERRORS"
            });

            var rows = csvReader.GetRecords<CsvApprenticeshipRowWithErrors>();
            rows.Should().BeEquivalentTo(
                new CsvApprenticeshipRowWithErrors()
                {
                    StandardCode = apprenticeshipUploadRows[0].StandardCode.ToString(),
                    StandardVersion = apprenticeshipUploadRows[0].StandardVersion.ToString(),
                    ApprenticeshipInformation = apprenticeshipUploadRows[0].ApprenticeshipInformation,
                    ApprenticeshipWebpage = apprenticeshipUploadRows[0].ApprenticeshipWebpage,
                    ContactEmail = apprenticeshipUploadRows[0].ContactEmail,
                    ContactPhone = apprenticeshipUploadRows[0].ContactPhone,
                    ContactUrl = apprenticeshipUploadRows[0].ContactUrl,
                    DeliveryMethod = apprenticeshipUploadRows[0].DeliveryMethod,
                    VenueName = apprenticeshipUploadRows[0].VenueName,
                    YourVenueReference = apprenticeshipUploadRows[0].YourVenueReference,
                    Radius = apprenticeshipUploadRows[0].Radius,
                    DeliveryModes = apprenticeshipUploadRows[0].DeliveryModes,
                    NationalDelivery = apprenticeshipUploadRows[0].NationalDelivery,
                    SubRegion = apprenticeshipUploadRows[0].SubRegions,
                    Errors = ErrorRegistry.All["APPRENTICESHIP_TELEPHONE_REQUIRED"].GetMessage() + "\n"
                        + ErrorRegistry.All["APPRENTICESHIP_INFORMATION_REQUIRED"].GetMessage()
                });
        }
    }
}
