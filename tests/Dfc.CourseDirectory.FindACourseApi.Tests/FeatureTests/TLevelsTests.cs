﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;


namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class TLevelsTests : TestBase
    {
        public TLevelsTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task TLevels_Get_WithNoTLevels_ReturnsOkWithEmptyCollection()
        {
            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetTLevels>()))
                .ReturnsAsync(Array.Empty<TLevel>());



            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetProvidersByIds>()))
                .ReturnsAsync(new Dictionary<Guid, Provider>());

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetProviderContactById>()))
                .ReturnsAsync(new ProviderContact());

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetVenuesByIds>()))
                .ReturnsAsync(new Dictionary<Guid, Venue>());


            // Act
            var response = await HttpClient.GetAsync($"tlevels");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            result.Should().NotBeNull();
            result["tLevels"].ToArray().Should().BeEmpty();
        }

        [Fact]
        public async Task TLevels_Get_WithTLevels_ReturnsOkWithExpectedTLevelsCollection()
        {
            var provider1 = CreateProvider(1);
            var provider1Contact = CreateProviderContact(provider1, 1);
            var provider1Venue1 = CreateVenue(1);
            var provider1Venue2 = CreateVenue(2);
            var provider1TLevelLocation1 = CreateTLevelLocation(1, provider1Venue1.VenueId);
            var provider1TLevelLocation2 = CreateTLevelLocation(2, provider1Venue1.VenueId);
            var provider1TLevelLocation3 = CreateTLevelLocation(3, provider1Venue2.VenueId);

            // Provider1, with one location
            var provider1TLevel1 = CreateTLevel(1, provider1, new[] { provider1TLevelLocation1 });
            // Provider1, with two locations
            var provider1TLevel2 = CreateTLevel(2, provider1, new[] { provider1TLevelLocation2, provider1TLevelLocation3 });

            var provider2 = CreateProvider(2);
            var provider2Contact = CreateProviderContact(provider2, 2);
            var provider2Venue1 = CreateVenue(3);
            var provider2TLevelLocation1 = CreateTLevelLocation(4, provider2Venue1.VenueId);
            
            // Provider2, with one location, no FeChoices
            var provider2TLevel1 = CreateTLevel(3, provider2, new[] { provider2TLevelLocation1 });

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetTLevels>()))
                .ReturnsAsync(new[] { provider1TLevel1, provider1TLevel2, provider2TLevel1 });


            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetProvidersByIds>()))
                .ReturnsAsync(new[] { provider1, provider2 }.ToDictionary(p => p.ProviderId, p => p));

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetProviderContactById>()))
                .ReturnsAsync(provider1Contact);



            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetVenuesByIds>()))
                .ReturnsAsync(new[] { provider1Venue1, provider1Venue2, provider2Venue1 }.ToDictionary(v => v.VenueId, v => v));

            

            // Act
            var response = await HttpClient.GetAsync($"tlevels");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            result.Should().NotBeNull();
            var tLevels = result["tLevels"].ToArray();
            tLevels.Should().NotBeNullOrEmpty();
            tLevels.Length.Should().Be(3);

            AssertHasTLevel(tLevels, provider1, provider1Contact, new[] { provider1Venue1, provider1Venue2 }, provider1TLevel1);
            AssertHasTLevel(tLevels, provider1, provider1Contact, new[] { provider1Venue1, provider1Venue2 }, provider1TLevel2);

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetProviderContactById>()))
            .ReturnsAsync(provider2Contact);
             response = await HttpClient.GetAsync($"tlevels");
             result = JObject.Parse(await response.Content.ReadAsStringAsync());
             tLevels = result["tLevels"].ToArray();

            AssertHasTLevel(tLevels, provider2, provider2Contact, new[] { provider2Venue1 }, provider2TLevel1);

            static void AssertHasTLevel(JToken[] tLevels, Provider provider, ProviderContact providerContact, IReadOnlyCollection<Venue> venues, TLevel expectedTLevel)
            {
                var tLevel = tLevels.SingleOrDefault(t => t["tLevelId"].ToObject<Guid>() == expectedTLevel.TLevelId);
                tLevel.Should().NotBeNull();


                using (new AssertionScope())
                {
                    tLevel["offeringType"].ToObject<string>().Should().Be("TLevel");
                    tLevel["tLevelDefinitionId"].ToObject<Guid>().Should().Be(expectedTLevel.TLevelDefinition.TLevelDefinitionId);
                    tLevel["qualification"]["tLevelName"].ToObject<string>().Should().Be(expectedTLevel.TLevelDefinition.Name);
                    tLevel["qualification"]["qualificationLevel"].ToObject<string>().Should().Be(expectedTLevel.TLevelDefinition.QualificationLevel.ToString());
                    tLevel["qualification"]["frameworkCode"].ToObject<int>().Should().Be(expectedTLevel.TLevelDefinition.FrameworkCode);
                    tLevel["qualification"]["progType"].ToObject<int>().Should().Be(expectedTLevel.TLevelDefinition.ProgType);
                    tLevel["provider"]["providerName"].ToObject<string>().Should().Be(provider.DisplayName);
                    tLevel["provider"]["ukprn"].ToObject<int>().Should().Be(provider.Ukprn);
                    tLevel["provider"]["addressLine1"].ToObject<string>().Should().Be($"{providerContact?.AddressSaonDescription} {providerContact?.AddressPaonDescription} {providerContact?.AddressStreetDescription}");
                    tLevel["provider"]["addressLine2"].ToObject<string>().Should().Be(providerContact?.AddressLocality);
                    tLevel["provider"]["town"].ToObject<string>().Should().Be(providerContact?.AddressPostTown);
                    tLevel["provider"]["postcode"].ToObject<string>().Should().Be(providerContact?.AddressPostcode);
                    tLevel["provider"]["county"].ToObject<string>().Should().Be(providerContact?.AddressCounty);
                    tLevel["provider"]["email"].ToObject<string>().Should().Be(providerContact?.Email);
                    tLevel["provider"]["telephone"].ToObject<string>().Should().Be(providerContact?.Telephone1);
                    tLevel["provider"]["fax"].ToObject<string>().Should().Be(providerContact?.Fax);
                    tLevel["provider"]["website"].ToObject<string>().Should().Be($"http://{providerContact?.WebsiteAddress}");
                    tLevel["whoFor"].ToObject<string>().Should().Be(expectedTLevel.WhoFor);
                    tLevel["entryRequirements"].ToObject<string>().Should().Be(expectedTLevel.EntryRequirements);
                    tLevel["whatYoullLearn"].ToObject<string>().Should().Be(expectedTLevel.WhatYoullLearn);
                    tLevel["howYoullLearn"].ToObject<string>().Should().Be(expectedTLevel.HowYoullLearn);
                    tLevel["howYoullBeAssessed"].ToObject<string>().Should().Be(expectedTLevel.HowYoullBeAssessed);
                    tLevel["whatYouCanDoNext"].ToObject<string>().Should().Be(expectedTLevel.WhatYouCanDoNext);
                    tLevel["website"].ToObject<string>().Should().Be($"http://{expectedTLevel.Website}");
                    tLevel["startDate"].ToObject<DateTime>().Should().Be(expectedTLevel.StartDate);
                    tLevel["deliveryMode"].ToObject<string>().Should().Be("ClassroomBased");
                    tLevel["attendancePattern"].ToObject<string>().Should().Be("Daytime");
                    tLevel["studyMode"].ToObject<string>().Should().Be("FullTime");
                    tLevel["durationUnit"].ToObject<string>().Should().Be("Years");
                    tLevel["durationValue"].ToObject<int>().Should().Be(2);
                    tLevel["cost"].ToObject<decimal?>().Should().BeNull();
                    tLevel["costDescription"].ToObject<string>().Should().Be("T Levels are currently only available to 16-19 year olds. Contact us for details of other suitable courses.");

                    var locations = tLevel["locations"].ToArray();
                    locations.Length.Should().Be(expectedTLevel.Locations.Count);

                    foreach (var expectedLocation in expectedTLevel.Locations)
                    {
                        var location = locations.SingleOrDefault(l => l["tLevelLocationId"].ToObject<Guid>() == expectedLocation.TLevelLocationId);
                        location.Should().NotBeNull();

                        var expectedVenue = venues.Single(v => v.VenueId == expectedLocation.VenueId);

                        location["venueName"].ToObject<string>().Should().Be(expectedVenue.VenueName);
                        location["addressLine1"].ToObject<string>().Should().Be(expectedVenue.AddressLine1);
                        location["addressLine2"].ToObject<string>().Should().Be(expectedVenue.AddressLine2);
                        location["town"].ToObject<string>().Should().Be(expectedVenue.Town);
                        location["county"].ToObject<string>().Should().Be(expectedVenue.County);
                        location["postcode"].ToObject<string>().Should().Be(expectedVenue.Postcode);
                        location["telephone"].ToObject<string>().Should().Be(expectedVenue.Telephone);
                        location["email"].ToObject<string>().Should().Be(expectedVenue.Email);
                        location["website"].ToObject<string>().Should().Be($"http://{expectedVenue.Website}");
                        location["latitude"].ToObject<decimal>().Should().Be(Convert.ToDecimal(expectedVenue.Latitude));
                        location["longitude"].ToObject<decimal>().Should().Be(Convert.ToDecimal(expectedVenue.Longitude));
                    }
                }
            }
        }

        private static Provider CreateProvider(int seed) =>
            new Provider
            {
                ProviderId = Guid.NewGuid(),
                Ukprn = (1234 * seed),
                ProviderName = $"TestProviderName{seed}"

            };

        private static ProviderContact CreateProviderContact(Provider provider, int seed) =>
            new ProviderContact
            {
                ProviderId = provider.ProviderId,
                ContactType = "P",
                AddressSaonDescription =  $"TestSAON{seed}",
                AddressPaonDescription = $"TestPAON{seed}",
                AddressStreetDescription = $"TestStreetDescription{seed}",
                AddressLocality = $"TestLocality{seed}",
                AddressItems = $"TestItemsTown{seed} "+$"TestItemsCounty{seed}",
                AddressPostTown = $"TestPostTown{seed}",
                AddressCounty = $"TestCounty{seed}",
                AddressPostcode = $"TestPostCode{seed}",
                Telephone1 = $"TestContactTelephone1{seed}",
                Fax = $"TestContactFax{seed}",
                WebsiteAddress = $"testing{seed}.com",
                Email = $"TestContactEmail{seed}"
            };

        private static Venue CreateVenue(int seed) =>
            new Venue
            {
                VenueId = Guid.NewGuid(),
                VenueName = $"TestVenueName{seed}",
                ProviderVenueRef = $"TestProviderVenueRef{seed}",
                AddressLine1 = $"TestAddressLine1{seed}",
                AddressLine2 = $"TestAddressLine2{seed}",
                Town = $"TestTown{seed}",
                County = $"TestCounty{seed}",
                Postcode = $"TestPostcode{seed}",
                Telephone = $"TestTelephone{seed}",
                Email = $"TestEmail{seed}",
                Website = $"TestWebsite{seed}",
                Latitude = 12 * seed,
                Longitude = 34 * seed
            };

        private static TLevel CreateTLevel(int seed, Provider provider, IEnumerable<TLevelLocation> locations) =>
            new TLevel
            {
                TLevelId = Guid.NewGuid(),
                TLevelStatus = TLevelStatus.Live,
                TLevelDefinition = new TLevelDefinition
                {
                    TLevelDefinitionId = Guid.NewGuid(),
                    FrameworkCode = 12 * seed,
                    ProgType = 34 * seed,
                    QualificationLevel = 56 * seed,
                    Name = $"TestTLevelDefinition{seed}"
                },
                ProviderId = provider.ProviderId,
                ProviderName = provider.ProviderName,
                WhoFor = $"TestWhoFor{seed}",
                EntryRequirements = $"TestEntryRequirements{seed}",
                WhatYoullLearn = $"TestWhatYoullLearn{seed}",
                HowYoullLearn = $"TestHowYoullLearn{seed}",
                HowYoullBeAssessed = $"TestHowYoullBeAssessed{seed}",
                WhatYouCanDoNext = $"TestWhatYouCanDoNext{seed}",
                YourReference = $"TestYourReference{seed}",
                StartDate = DateTime.UtcNow.AddDays(seed),
                Locations = locations?.ToList(),
                Website = $"TestWebsite{seed}",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

        private static TLevelLocation CreateTLevelLocation(int seed, Guid venueId) =>
            new TLevelLocation
            {
                TLevelLocationId = Guid.NewGuid(),
                TLevelLocationStatus = TLevelLocationStatus.Live,
                VenueId = venueId,
                VenueName = $"TestVenueName{seed}"
            };

        
    }
}
