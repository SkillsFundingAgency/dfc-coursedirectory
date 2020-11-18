﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.EditVenue;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.EditVenue
{
    public class AddressTests : MvcTestBase
    {
        public AddressTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(
                providerId,
                addressLine1: "Test Venue line 1",
                addressLine2: "Test Venue line 2",
                town: "Town",
                county: "County",
                postcode: "AB1 2DE");

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/address");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("AddressLine1").GetAttribute("value").Should().Be("Test Venue line 1");
                doc.GetElementById("AddressLine2").GetAttribute("value").Should().Be("Test Venue line 2");
                doc.GetElementById("Town").GetAttribute("value").Should().Be("Town");
                doc.GetElementById("County").GetAttribute("value").Should().Be("County");
                doc.GetElementById("Postcode").GetAttribute("value").Should().Be("AB1 2DE");
            }
        }

        [Fact]
        public async Task Get_NewAddressAlreadySetInFormFlowInstance_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var formFlowInstance = await CreateFormFlowInstance(venueId);
            formFlowInstance.UpdateState(state =>
            {
                state.AddressLine1 = "Updated line 1";
                state.AddressLine2 = "Updated line 2";
                state.Town = "Updated town";
                state.County = "Updated county";
                state.Postcode = "UP1 D8D";
            });

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/address");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("AddressLine1").GetAttribute("value").Should().Be("Updated line 1");
                doc.GetElementById("AddressLine2").GetAttribute("value").Should().Be("Updated line 2");
                doc.GetElementById("Town").GetAttribute("value").Should().Be("Updated town");
                doc.GetElementById("County").GetAttribute("value").Should().Be("Updated county");
                doc.GetElementById("Postcode").GetAttribute("value").Should().Be("UP1 D8D");
            }
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessVenue_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            var venueId = await TestData.CreateVenue(providerId);

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == "CV1 2AA")))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = new[]
                    {
                        new SearchResultItem<Onspd>()
                        {
                            Record = new Onspd()
                            {
                                pcds = "CV1 2AA",
                                Country = "England",
                                lat = 42M,
                                @long = 43M
                            }
                        }
                    }
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", "Updated address line 1")
                .Add("AddressLine2", "Updated address line 2")
                .Add("Town", "Updated town")
                .Add("County", "Updated county")
                .Add("Postcode", "CV1 2AA")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_VenueDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();

            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == "CV1 2AA")))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = new[]
                    {
                        new SearchResultItem<Onspd>()
                        {
                            Record = new Onspd()
                            {
                                pcds = "CV1 2AA",
                                Country = "England",
                                lat = 42M,
                                @long = 43M
                            }
                        }
                    }
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", "Updated address line 1")
                .Add("AddressLine2", "Updated address line 2")
                .Add("Town", "Updated town")
                .Add("County", "Updated county")
                .Add("Postcode", "CV1 2AA")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [MemberData(nameof(InvalidAddressData))]
        public async Task Post_InvalidAddress_RendersError(
            string addressLine1,
            string addressLine2,
            string town,
            string county,
            string postcode,
            string expectedErrorInputId,
            string expectedErrorMessage)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == "CV1 2AA")))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = new[]
                    {
                        new SearchResultItem<Onspd>()
                        {
                            Record = new Onspd()
                            {
                                pcds = postcode,
                                Country = "England",
                                lat = 42M,
                                @long = 43M
                            }
                        }
                    }
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", addressLine1)
                .Add("AddressLine2", addressLine2)
                .Add("Town", town)
                .Add("County", county)
                .Add("Postcode", postcode)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(expectedErrorInputId, expectedErrorMessage);
        }

        [Theory]
        [InlineData("England", false)]
        [InlineData("Scotland", true)]
        public async Task Post_ValidRequest_UpdatesFormFlowInstanceAndRedirects(
            string onspdRecordPostcode,
            bool expectedNewAddressIsOutsideOfEnglandValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == "CV1 2AA")))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = new[]
                    {
                        new SearchResultItem<Onspd>()
                        {
                            Record = new Onspd()
                            {
                                pcds = "CV1 2AA",
                                Country = onspdRecordPostcode,
                                lat = 42M,
                                @long = 43M
                            }
                        }
                    }
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", "Updated address line 1")
                .Add("AddressLine2", "Updated address line 2")
                .Add("Town", "Updated town")
                .Add("County", "Updated county")
                .Add("Postcode", "CV1 2AA")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.OriginalString.Should().Be($"/venues/{venueId}");

            using (new AssertionScope())
            {
                var formFlowInstance = GetFormFlowInstance(venueId);
                formFlowInstance.State.AddressLine1.Should().Be("Updated address line 1");
                formFlowInstance.State.AddressLine2.Should().Be("Updated address line 2");
                formFlowInstance.State.Town.Should().Be("Updated town");
                formFlowInstance.State.County.Should().Be("Updated county");
                formFlowInstance.State.Postcode.Should().Be("CV1 2AA");
                formFlowInstance.State.Latitude.Should().Be(42M);
                formFlowInstance.State.Longitude.Should().Be(43M);
                formFlowInstance.State.NewAddressIsOutsideOfEngland.Should().Be(expectedNewAddressIsOutsideOfEnglandValue);
            }
        }

        public static IEnumerable<object[]> InvalidAddressData { get; } =
            new[]
            {
                // Address line 1 missing
                (
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: "Enter address line 1"
                ),
                // Address line 1 too long
                (
                    AddressLine1: new string('x', 101),
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: "Address line 1 must be 100 characters or less"
                ),
                // Address line 1 has invalid characters
                (
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: "Address line 1 must only include letters a to z, numbers, hyphens and spaces"
                ),
                // Address line 2 too long
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: new string('x', 101),
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: "Address line 2 must be 100 characters or less"
                ),
                // Address line 2 has invalid characters
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "!",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: "Address line 2 must only include letters a to z, numbers, hyphens and spaces"
                ),
                // Town missing
                (
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: "Enter a town or city"
                ),
                // Town is too long
                (
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: new string('x', 76),
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: "Town or city must be 75 characters or less"
                ),
                // Town has invalid characters
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "!",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: "Town or city must only include letters a to z, numbers, hyphens and spaces"
                ),
                // Postcode is not valid
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "X",
                    ExpectedErrorInputId: "Postcode",
                    ExpectedErrorMessage: "Enter a real postcode"
                )
            }
            .Select(t => new object[] { t.AddressLine1, t.AddressLine2, t.Town, t.County, t.Postcode, t.ExpectedErrorInputId, t.ExpectedErrorMessage })
            .ToArray();

        private async Task<FormFlowInstance<EditVenueFlowModel>> CreateFormFlowInstance(Guid venueId)
        {
            var state = await Factory.Services.GetRequiredService<EditVenueFlowModelFactory>()
                .CreateModel(venueId);

            return CreateFormFlowInstanceForRouteParameters(
                key: "EditVenue",
                routeParameters: new Dictionary<string, object>()
                {
                    { "venueId", venueId }
                },
                state);
        }

        private FormFlowInstance<EditVenueFlowModel> GetFormFlowInstance(Guid venueId) =>
            GetFormFlowInstanceForRouteParameters<EditVenueFlowModel>(
                key: "EditVenue",
                routeParameters: new Dictionary<string, object>()
                {
                    { "venueId", venueId }
                });
    }
}
