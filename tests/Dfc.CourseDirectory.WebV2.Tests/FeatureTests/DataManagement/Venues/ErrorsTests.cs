using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class ErrorsTests : MvcTestBase
    {
        public ErrorsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_NoUnpublishedVenueUpload_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_FileHasNoErrors_ReturnsRedirect()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddValidRows(1);
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/data-upload/venues/check-publish?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddValidRows(1);

                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                // Should only have one error row in the table
                var errorRowsTable = doc.GetElementByTestId("ErrorRows");
                var rows = errorRowsTable.QuerySelector("tbody").QuerySelectorAll("tr");
                rows.Count().Should().Be(1);

                var x = rows.First().ChildNodes;
                var firstRowCells = rows.First().QuerySelectorAll("th,td").Select(n => n.TextContent.Trim());
                firstRowCells.Should().BeEquivalentTo(new[]
                {
                    venueUploadRows[1].ProviderVenueRef,
                    venueUploadRows[1].VenueName,
                    venueUploadRows[1].AddressLine1 + ", " + venueUploadRows[1].AddressLine2 + ", " + venueUploadRows[1].Town + ", " + venueUploadRows[1].County + ", " + venueUploadRows[1].Postcode,
                    Core.DataManagement.Errors.MapVenueErrorToFieldGroup("VENUE_NAME_REQUIRED")
                });

                doc.GetElementByTestId("ResolveOnScreenOption").Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_MoreThan30RowsWithErrors_DoesNotRender_ResolveOnScreenOption()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    for (int i = 0; i < 31; i++)
                    {
                        rowBuilder.AddRow(record =>
                        {
                            record.VenueName = string.Empty;
                            record.IsValid = false;
                            record.Errors = new[]
                            {
                                ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode,
                            };
                        });
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ResolveOnScreenOption").Should().BeNull();
        }

        [Fact]
        public async Task Post_MissingWhatNext_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/errors?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("WhatNext", "Select what you want to do");
        }

        [Theory]
        [InlineData("ResolveOnScreen", "/data-upload/venues/resolve?providerId={0}")]
        [InlineData("UploadNewFile", "/data-upload/venues?providerId={0}")]
        [InlineData("DeleteUpload", "/data-upload/venues/delete?providerId={0}")]
        public async Task Post_ValidRequest_ReturnsRedirect(string selectedOption, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/errors?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhatNext", selectedOption)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));
        }
    }
}
