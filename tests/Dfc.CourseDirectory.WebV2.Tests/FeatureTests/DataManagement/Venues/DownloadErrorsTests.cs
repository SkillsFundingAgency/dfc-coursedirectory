using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class DownloadErrorsTests : MvcTestBase
    {
        public DownloadErrorsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_ProviderHasNoVenueUploadAtProcessedWithErrorsStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/download-errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsRowsWithErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");

            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = string.Empty;
                        record.Postcode = "x";
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["VENUE_POSTCODE_FORMAT"].ErrorCode
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/download-errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_venues_errors_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.HeaderRecord.Should().BeEquivalentTo(new[]
            {
                "YOUR_VENUE_REFERENCE",
                "VENUE_NAME",
                "ADDRESS_LINE_1",
                "ADDRESS_LINE_2",
                "TOWN_OR_CITY",
                "COUNTY",
                "POSTCODE",
                "EMAIL",
                "PHONE",
                "WEBSITE",
                "ERRORS"
            });

            var rows = csvReader.GetRecords<CsvVenueRowWithErrors>();
            rows.Should().BeEquivalentTo(
                new CsvVenueRowWithErrors()
                {
                    ProviderVenueRef = venueUploadRows[0].ProviderVenueRef,
                    VenueName = venueUploadRows[0].VenueName,
                    AddressLine1 = venueUploadRows[0].AddressLine1,
                    AddressLine2 = venueUploadRows[0].AddressLine2,
                    Town = venueUploadRows[0].Town,
                    County = venueUploadRows[0].County,
                    Postcode = venueUploadRows[0].Postcode,
                    Email = venueUploadRows[0].Email,
                    Telephone = venueUploadRows[0].Telephone,
                    Website = venueUploadRows[0].Website,
                    Errors = ErrorRegistry.All["VENUE_NAME_REQUIRED"].GetMessage(ErrorMessageContext.DataManagement) + "\n"
                        + ErrorRegistry.All["VENUE_POSTCODE_FORMAT"].GetMessage(ErrorMessageContext.DataManagement)
                });
        }
    }
}
