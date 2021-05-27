using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class DeleteVenueTests : MvcTestBase
    {
        public DeleteVenueTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(54)]
        public async Task Get_DeleteNonExistentRowNumber_ReturnsError(int rowNumber)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (_, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/resolve/{rowNumber}/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_DeleteRowForNonExistentFileUpload_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/resolve/1/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_DeleteExistingRowWithErrors_ReturnsOK()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (_, rows) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);
            var errorRow = rows.FirstOrDefault();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/resolve/{errorRow.RowNumber}/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response.GetDocument();
            var row = doc.GetElementByTestId($"row-{errorRow.RowNumber}");
            using (new AssertionScope())
            {
                row.Should().NotBeNull();
                row.GetElementByTestId("YourRef").TextContent.Should().Be(errorRow.ProviderVenueRef);
                row.GetElementByTestId("VenueName").TextContent.Should().Be(errorRow.VenueName);
                row.GetElementByTestId("Address").TextContent.Should().Be(FormatAddress(errorRow));
                row.GetElementByTestId("Errors").TextContent.Should().Be(GetUniqueErrorMessages(errorRow));
            }
        }

        [Fact]
        public async Task Get_DeletedRow_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, rows) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);
            var errorRow = rows.FirstOrDefault();

            // Act
            var getResponse1 = await HttpClient.GetAsync($"/data-upload/venues/resolve/{errorRow.RowNumber}/delete");
            var postRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var postResponse = await HttpClient.SendAsync(postRequest);
            var getResponse2 = await HttpClient.GetAsync($"/data-upload/venues/resolve/{errorRow.RowNumber}/delete");

            // Assert
            getResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            getResponse2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_DeleteExistingRowWithAddressError_ReturnsOK()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = "someVenue";
                        record.AddressLine1 = "";
                        record.AddressLine2 = "kF31RZBDa4tsCGqBxlrNELlPxttbgQPrEHlh0Ifm80ziYbQNFf7Mrl4bxengIlgOonizCh33B1oo4pmV6WAHZMTxdkoWZwfIKXHxJleZjI6nD8lnT1xxUzYv";
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_ADDRESS_LINE1_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["VENUE_ADDRESS_LINE2_MAXLENGTH"].ErrorCode
                        };
                    });
                });
            var errorRow = venueUploadRows.FirstOrDefault();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/resolve/{errorRow.RowNumber}/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response.GetDocument();
            var row = doc.GetElementByTestId($"row-{errorRow.RowNumber}");
            using (new AssertionScope())
            {
                row.Should().NotBeNull();
                row.GetElementByTestId("YourRef").TextContent.Should().Be(errorRow.ProviderVenueRef);
                row.GetElementByTestId("VenueName").TextContent.Should().Be(errorRow.VenueName);
                row.GetElementByTestId("Address").TextContent.Should().Be(FormatAddress(errorRow));
                row.GetElementByTestId("Errors").TextContent.Should().Be(GetUniqueErrorMessages(errorRow));
            }
        }

        [Fact]
        public async Task Get_DeleteExistingRowWithMultipleErrors_ReturnsOK()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.AddressLine1 = "@@2343fdsfdsfsdsdf";
                        record.AddressLine2 = "@@2343fdsfdsfsdsdf";
                        record.County = "@@@@@@@-3423";
                        record.Email = "someFakeEmail@@someInvalid@email.com";
                        record.Postcode = "AAAAAAA AAAAAAA";
                        record.Telephone = "-1-1-1-1";
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_COUNTY_FORMAT"].ErrorCode,
                            ErrorRegistry.All["VENUE_EMAIL_FORMAT"].ErrorCode,
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["VENUE_POSTCODE_FORMAT"].ErrorCode,
                            ErrorRegistry.All["VENUE_PROVIDER_VENUE_REF_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["VENUE_TELEPHONE_FORMAT"].ErrorCode,
                        };
                    });
                });
            var errorRow = venueUploadRows.FirstOrDefault();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/resolve/{errorRow.RowNumber}/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response.GetDocument();
            var row = doc.GetElementByTestId($"row-{errorRow.RowNumber}");
            using (new AssertionScope())
            {
                row.Should().NotBeNull();
                row.GetElementByTestId("YourRef").TextContent.Should().Be(errorRow.ProviderVenueRef);
                row.GetElementByTestId("VenueName").TextContent.Should().Be(errorRow.VenueName);
                row.GetElementByTestId("Address").TextContent.Should().Be(FormatAddress(errorRow));
                row.GetElementByTestId("Errors").TextContent.Should().Be(GetUniqueErrorMessages(errorRow));
            }
        }

        [Fact]
        public async Task Get_DeleteExistingRowWithoutErrors_ReturnsOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (_, rows) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedSuccessfully);
            var row = rows.FirstOrDefault();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/resolve/{row.RowNumber}/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_DeleteExistingRow_ReturnsOK()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, rows) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);
            var errorRow = rows.FirstOrDefault();

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Fact]
        public async Task Post_DeleteExistingRowWithoutErrors_ReturnsOK()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
            provider.ProviderId,
            createdBy: User.ToUserInfo(),
            UploadStatus.ProcessedWithErrors,
            rowBuilder =>
            {
                rowBuilder.AddRow(record =>
                {
                    record.VenueName = "someVenue";
                    record.IsValid = true;
                });
            });
            var errorRow = venueUploadRows.FirstOrDefault();

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Fact]
        public async Task Post_DeleteLastVenueInError_RedirectsToPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
            provider.ProviderId,
            createdBy: User.ToUserInfo(),
            UploadStatus.ProcessedWithErrors,
            rowBuilder =>
            {
                rowBuilder.AddRow(record =>
                {
                    record.VenueName = "someVenue";
                    record.IsValid = false;
                });
            });
            var errorRow = venueUploadRows.FirstOrDefault();

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().Be($"/data-upload/venues/check-publish?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Post_DeleteLastVenueInError_RedirectsToResolveErrorList()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
            provider.ProviderId,
            createdBy: User.ToUserInfo(),
            UploadStatus.ProcessedWithErrors,
            rowBuilder =>
            {
                rowBuilder.AddRow(record =>
                {
                    record.VenueName = "someVenue";
                    record.IsValid = false;
                });
                rowBuilder.AddRow(record =>
                {
                    record.VenueName = "someVenuffffe";
                    record.IsValid = false;
                });
            });
            var errorRow = venueUploadRows.FirstOrDefault();

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().Be($"/data-upload/venues/check-publish?providerId={provider.ProviderId}");
        }


        [Fact]
        public async Task Post_DeletedRow_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
            provider.ProviderId,
            createdBy: User.ToUserInfo(),
            UploadStatus.ProcessedWithErrors,
            rowBuilder =>
            {
                rowBuilder.AddRow(record =>
                {
                    record.VenueName = "someVenue";
                    record.IsValid = true;
                });
            });
            var errorRow = venueUploadRows.FirstOrDefault();

            // Act
            var request1 = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Row", errorRow.RowNumber)
                .ToContent()
            };
            var response1 = await HttpClient.SendAsync(request1);
            var request2 = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
               .Add("Confirm", "true")
               .ToContent()
            };
            var response2 = await HttpClient.SendAsync(request2);

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.Found);
            response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_NotConfirmed_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
            provider.ProviderId,
            createdBy: User.ToUserInfo(),
            UploadStatus.ProcessedWithErrors,
            rowBuilder =>
            {
                rowBuilder.AddRow(record =>
                {
                    record.VenueName = "someVenue";
                    record.IsValid = true;
                });
            });
            var errorRow = venueUploadRows.FirstOrDefault();
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/{errorRow.RowNumber}/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
               .Add("Confirm", "false")
               .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete this venue");
        }

        [Fact]
        public async Task Post_DeleteNonExistingRow_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var (venueUpload, rows) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);
            var errorRow = rows.FirstOrDefault();

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/venues/resolve/242/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private string FormatAddress(VenueUploadRow row)
        {
            var addressParts = new List<string> { row.AddressLine1, row.AddressLine2, row.County, row.Postcode };
            var address = addressParts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            return string.Join(",", address);
        }

        private string GetUniqueErrorMessages(VenueUploadRow row)
        {
            var errors = row.Errors.Select(errorCode => Core.DataManagement.Errors.MapVenueErrorToFieldGroup(errorCode));
            return string.Join(",", errors.Distinct().ToList());
        }
    }
}
