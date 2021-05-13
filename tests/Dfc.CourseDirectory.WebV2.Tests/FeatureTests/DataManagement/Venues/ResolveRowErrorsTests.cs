using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class ResolveRowErrorsTests : MvcTestBase
    {
        public ResolveRowErrorsTests(CourseDirectoryApplicationFactory factory)
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
        public async Task Get_NoVenueUploadAtProcessedWithErrorsStatus_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/resolve/3?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.VenueName = string.Empty;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode
                        };
                        record.IsValid = false;
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/resolve/3?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_RowHasNoErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.IsValid = true;
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/resolve/2?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.VenueName = string.Empty;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode
                        };
                        record.IsValid = false;
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/resolve/2?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.AssertHasError("Name", ErrorRegistry.All["VENUE_NAME_REQUIRED"].GetMessage());
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_NoVenueUploadAtProcessedWithErrorsStatus_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var inputPostcode = "ab12de";

            var providerVenueRef = "VENUE";
            var name = "My Venue";
            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var country = "England";
            var normalizedPostcode = "AB1 2DE";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(normalizedPostcode, postcodeLatitude, postcodeLongitude, country == "England");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/resolve/2?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("ProviderVenueRef", providerVenueRef)
                    .Add("Name", name)
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", inputPostcode)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    // The row we're editing
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.IsValid = true;
                    });
                });

            var inputPostcode = "ab12de";

            var providerVenueRef = "VENUE";
            var name = "My Venue";
            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var country = "England";
            var normalizedPostcode = "AB1 2DE";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(normalizedPostcode, postcodeLatitude, postcodeLongitude, country == "England");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/resolve/3?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("ProviderVenueRef", providerVenueRef)
                    .Add("Name", name)
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", inputPostcode)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_RowHasNoErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    // The row we're editing
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.IsValid = true;
                    });
                });

            var inputPostcode = "ab12de";

            var providerVenueRef = "VENUE";
            var name = "My Venue";
            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var country = "England";
            var normalizedPostcode = "AB1 2DE";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(normalizedPostcode, postcodeLatitude, postcodeLongitude, country == "England");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/resolve/2?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("ProviderVenueRef", providerVenueRef)
                    .Add("Name", name)
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", inputPostcode)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [MemberData(nameof(InvalidFixData))]
        public async Task Post_WithErrors_RendersExpectedError(
            string providerVenueRef,
            string name,
            string addressLine1,
            string addressLine2,
            string town,
            string county,
            string postcode,
            string email,
            string telephone,
            string website,
            string expectedErrorInputId,
            string expectedErrorMessage)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    // The row we're editing
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.VenueName = string.Empty;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode
                        };
                        record.IsValid = false;
                    });

                    // Another row (for naming collision checks)
                    rowBuilder.AddRow(record =>
                    {
                        record.VenueName = "Existing Venue";
                        record.ProviderVenueRef = "EXISTING-REF";
                    });
                });

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, inEngland: true);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/resolve/2?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("ProviderVenueRef", providerVenueRef)
                    .Add("Name", name)
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", postcode)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(expectedErrorInputId, expectedErrorMessage);
        }

        [Theory]
        [InlineData(true, "/data-upload/venues/resolve?providerId={0}")]
        [InlineData(false, "/data-upload/venues/check-publish?providerId={0}")]
        public async Task Post_ValidRequest_UpdatesRowAndRedirects(bool otherRowsHaveErrors, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, venueUploadRows) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    // The row we're editing
                    rowBuilder.AddRow(record =>
                    {
                        record.RowNumber = 2;
                        record.VenueName = string.Empty;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode
                        };
                        record.IsValid = false;
                    });

                    if (otherRowsHaveErrors)
                    {
                        rowBuilder.AddRow(record =>
                        {
                            record.RowNumber = 3;
                            record.VenueName = string.Empty;
                            record.Errors = new[]
                            {
                                ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode
                            };
                            record.IsValid = false;
                        });
                    }
                });

            var inputPostcode = "ab12de";

            var providerVenueRef = "VENUE";
            var name = "My Venue";
            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var country = "England";
            var normalizedPostcode = "AB1 2DE";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(normalizedPostcode, postcodeLatitude, postcodeLongitude, country == "England");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/resolve/2?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("ProviderVenueRef", providerVenueRef)
                    .Add("Name", name)
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", inputPostcode)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));

            var (rows, _) = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId }));

            var updatedRow = rows.Single(r => r.RowNumber == 2);

            using (new AssertionScope())
            {
                updatedRow.ProviderVenueRef.Should().Be(providerVenueRef);
                updatedRow.VenueName.Should().Be(name);
                updatedRow.AddressLine1.Should().Be(addressLine1);
                updatedRow.AddressLine2.Should().Be(addressLine2);
                updatedRow.Town.Should().Be(town);
                updatedRow.County.Should().Be(county);
                updatedRow.Postcode.Should().Be(normalizedPostcode);
                updatedRow.Email.Should().Be(email);
                updatedRow.Telephone.Should().Be(telephone);
                updatedRow.Website.Should().Be(website);
            }
        }

        public static IEnumerable<object[]> InvalidFixData { get; } =
            new[]
            {
                // Name missing
                (
                    ProviderVenueRef: "REF",
                    Name: "",
                    AddressLine1: "",
                    AddressLine2: "Updated line 1",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Name",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_NAME_REQUIRED"]
                ),
                // Name is not unique for provider
                (
                    ProviderVenueRef: "REF",
                    Name: "Existing Venue",
                    AddressLine1: "",
                    AddressLine2: "Updated line 1",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Name",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_NAME_UNIQUE"]
                ),
                // Name is too long
                (
                    ProviderVenueRef: "REF",
                    Name: new string('x', 251),
                    AddressLine1: "",
                    AddressLine2: "Updated line 1",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Name",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_NAME_MAXLENGTH"]
                ),
                // Reference missing
                (
                    ProviderVenueRef: "",
                    Name: "Venue",
                    AddressLine1: "",
                    AddressLine2: "Updated line 1",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "ProviderVenueRef",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_PROVIDER_VENUE_REF_REQUIRED"]
                ),
                // Reference is not unique for provider
                (
                    ProviderVenueRef: "EXISTING-REF",
                    Name: "Venue",
                    AddressLine1: "",
                    AddressLine2: "Updated line 1",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "ProviderVenueRef",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_PROVIDER_VENUE_REF_UNIQUE"]
                ),
                // Reference is too long
                (
                    ProviderVenueRef: new string('x', 256),
                    Name: "Venue",
                    AddressLine1: "",
                    AddressLine2: "Updated line 1",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "ProviderVenueRef",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_PROVIDER_VENUE_REF_MAXLENGTH"]
                ),
                // Address line 1 missing
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE1_REQUIRED"]
                ),
                // Address line 1 too long
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: new string('x', 101),
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE1_MAXLENGTH"]
                ),
                // Address line 1 has invalid characters
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE1_FORMAT"]
                ),
                // Address line 2 too long
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: new string('x', 101),
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE2_MAXLENGTH"]
                ),
                // Address line 2 has invalid characters
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: "!",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE2_FORMAT"]
                ),
                // Town missing
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TOWN_REQUIRED"]
                ),
                // Town is too long
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: new string('x', 76),
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TOWN_MAXLENGTH"]
                ),
                // Town has invalid characters
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "!",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TOWN_FORMAT"]
                ),
                // Postcode is not valid
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "X",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Postcode",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_POSTCODE_FORMAT"]
                ),
                // Invalid Email
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "xxx",
                    Telephone: "01234 567890",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Email",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_EMAIL_FORMAT"]
                ),
                // Invalid PhoneNumber
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "xxx",
                    Website: "provider.com/venue",
                    ExpectedErrorInputId: "Telephone",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TELEPHONE_FORMAT"]
                ),
                // Invalid Website
                (
                    ProviderVenueRef: "REF",
                    Name: "Venue",
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    Email: "venue@provider.com",
                    Telephone: "01234 567890",
                    Website: "@bad/website",
                    ExpectedErrorInputId: "Website",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_WEBSITE_FORMAT"]
                )
            }
            .Select(t => new object[]
            {
                t.ProviderVenueRef,
                t.Name,
                t.AddressLine1,
                t.AddressLine2,
                t.Town,
                t.County,
                t.Postcode,
                t.Email,
                t.Telephone,
                t.Website,
                t.ExpectedErrorInputId,
                t.ExpectedErrorMessage })
            .ToArray();
    }
}
