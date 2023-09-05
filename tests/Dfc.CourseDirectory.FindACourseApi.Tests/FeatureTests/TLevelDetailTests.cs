using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class TLevelDetailTests : TestBase
    {
        public TLevelDetailTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task TLevelDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tLevelId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync($"tleveldetail?tlevelid={tLevelId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ValidRequest_ReturnsOkWithExpectedOutput()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var providerName = "Test Provider";
            var providerUkprn = 123456;
            var providerContactEmail = "contact@exampleprovider.com";
            var providerEmployerSatisfaction = 9m;
            var providerLearnerSatisfaction = 10m;

            var tLevelDefinitionId = Guid.NewGuid();
            var tLevelDefinitionName = "T Level name";
            var tLevelDefinitionFrameworkCode = 4;
            var tLevelDefinitionProgType = 3;
            var tLevelDefinitionQualificationLevel = 5;

            var venueId = Guid.NewGuid();
            var venueName = "Venue";
            var venueAddressLine1 = "Venue address line 1";
            var venueAddressLine2 = "Venue address line 2";
            var venueTown = "Venue town";
            var venueCounty = "Venue county";
            var venuePostcode = "AB1 2DE";
            var venueTelephone = "01234 567890";
            var venueEmail = "venue@exampleprovider.com";
            var venueWebsite = "exampleprovider.com/venue";
            var venueLatitude = 1d;
            var venueLongitude = 2d;

            var tLevelId = Guid.NewGuid();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";
            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://exampleprovider.com/tlevel";

            var tLevelLocationId = Guid.NewGuid();

            var now = DateTime.UtcNow;

            SqlQueryDispatcher
                .Setup(d => d.ExecuteQuery(It.Is<GetTLevel>(q => q.TLevelId == tLevelId)))
                .ReturnsAsync(new TLevel()
                {
                     TLevelId = tLevelId,
                     TLevelStatus = TLevelStatus.Live,
                     TLevelDefinition = new TLevelDefinition()
                     {
                         TLevelDefinitionId = tLevelDefinitionId,
                         Name = tLevelDefinitionName,
                         FrameworkCode = tLevelDefinitionFrameworkCode,
                         ProgType = tLevelDefinitionProgType,
                         QualificationLevel = tLevelDefinitionQualificationLevel
                     },
                     ProviderId = providerId,
                     ProviderName = providerName,
                     WhoFor = whoFor,
                     EntryRequirements = entryRequirements,
                     WhatYoullLearn = whatYoullLearn,
                     HowYoullLearn = howYoullLearn,
                     HowYoullBeAssessed = howYoullBeAssessed,
                     WhatYouCanDoNext = whatYouCanDoNext,
                     YourReference = yourReference,
                     StartDate = startDate,
                     Locations = new[]
                     {
                         new TLevelLocation()
                         {
                             TLevelLocationId = tLevelLocationId,
                             TLevelLocationStatus = TLevelLocationStatus.Live,
                             VenueId = venueId,
                             VenueName = venueName
                         }
                     },
                     Website = website,
                     CreatedOn = now,
                     UpdatedOn = now
                });

            SqlQueryDispatcher
                .Setup(d => d.ExecuteQuery(It.Is<GetProviderById>(q => q.ProviderId == providerId)))
                .ReturnsAsync(new Provider()
                {
                    ProviderId = providerId,
                    ProviderType = ProviderType.TLevels,
                    ProviderName = providerName,
                    Ukprn = providerUkprn
                });

            SqlQueryDispatcher
                .Setup(d => d.ExecuteQuery(It.Is<GetVenuesByIds>(q => q.VenueIds.SingleOrDefault() == venueId)))
                .ReturnsAsync(new Dictionary<Guid, Venue>()
                {
                    {
                        venueId,
                        new Venue()
                        {
                            VenueId = venueId,
                            VenueName = venueName,
                            AddressLine1 = venueAddressLine1,
                            AddressLine2 = venueAddressLine2,
                            Town = venueTown,
                            County = venueCounty,
                            Postcode = venuePostcode,
                            Telephone = venueTelephone,
                            Email = venueEmail,
                            Website = venueWebsite,
                            Latitude = venueLatitude,
                            Longitude = venueLongitude
                        }
                    }
                });

            SqlQueryDispatcher
                .Setup(d => d.ExecuteQuery(It.Is<Core.DataStore.Sql.Queries.GetProviderById>(q => q.ProviderId == providerId)))
                .ReturnsAsync(new Core.DataStore.Sql.Models.Provider()
                {
                    ProviderId = providerId,
                    Ukprn = providerUkprn,
                    ProviderType = ProviderType.TLevels,
                    EmployerSatisfaction = providerEmployerSatisfaction,
                    LearnerSatisfaction = providerLearnerSatisfaction

                });

            SqlQueryDispatcher
                .Setup(d => d.ExecuteQuery(It.Is<Core.DataStore.Sql.Queries.GetProviderContactById>(q => q.ProviderId == providerId)))
                .ReturnsAsync(new Core.DataStore.Sql.Models.ProviderContact()
                {
                    ProviderId = providerId,
                    Email = providerContactEmail,
                    ContactType = "P"
                });



            // Act
            var response = await HttpClient.GetAsync($"tleveldetail?tlevelid={tLevelId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            using (new AssertionScope())
            {
                json["offeringType"].ToObject<string>().Should().Be("TLevel");
                json["tLevelId"].ToObject<string>().Should().Be(tLevelId.ToString());
                json["tLevelDefinitionId"].ToObject<string>().Should().Be(tLevelDefinitionId.ToString());
                json["qualification"]["frameworkCode"].ToObject<int>().Should().Be(tLevelDefinitionFrameworkCode);
                json["qualification"]["progType"].ToObject<int>().Should().Be(tLevelDefinitionProgType);
                json["qualification"]["qualificationLevel"].ToObject<string>().Should().Be(tLevelDefinitionQualificationLevel.ToString());
                json["qualification"]["tLevelName"].ToObject<string>().Should().Be(tLevelDefinitionName);
                json["provider"]["providerName"].ToObject<string>().Should().Be(providerName);
                json["provider"]["ukprn"].ToObject<int>().Should().Be(providerUkprn);
                json["provider"]["email"].ToObject<string>().Should().Be(providerContactEmail);
                json["provider"]["employerSatisfaction"].ToObject<decimal?>().Should().Be(providerEmployerSatisfaction);
                json["provider"]["learnerSatisfaction"].ToObject<decimal?>().Should().Be(providerLearnerSatisfaction);
                json["whoFor"].ToObject<string>().Should().Be(whoFor);
                json["entryRequirements"].ToObject<string>().Should().Be(HtmlEncode(entryRequirements));
                json["whatYoullLearn"].ToObject<string>().Should().Be(HtmlEncode(whatYoullLearn));
                json["howYoullLearn"].ToObject<string>().Should().Be(HtmlEncode(howYoullLearn));
                json["howYoullBeAssessed"].ToObject<string>().Should().Be(HtmlEncode(howYoullBeAssessed));
                json["whatYouCanDoNext"].ToObject<string>().Should().Be(HtmlEncode(whatYouCanDoNext));
                json["website"].ToObject<string>().Should().Be(website);
                json["startDate"].ToObject<DateTime>().Should().Be(startDate);
                json["deliveryMode"].ToObject<string>().Should().Be("ClassroomBased");
                json["attendancePattern"].ToObject<string>().Should().Be("Daytime");
                json["studyMode"].ToObject<string>().Should().Be("FullTime");
                json["durationUnit"].ToObject<string>().Should().Be("Years");
                json["durationValue"].ToObject<int>().Should().Be(2);
                json["cost"].ToObject<decimal?>().Should().BeNull();
                json["costDescription"].ToObject<string>().Should().Be("T Levels are currently only available to 16-19 year olds. Contact us for details of other suitable courses.");

                var location = json["locations"].ToArray().Should().ContainSingle().Subject;
                location["tLevelLocationId"].ToObject<string>().Should().Be(tLevelLocationId.ToString());
                location["venueName"].ToObject<string>().Should().Be(venueName);
                location["addressLine1"].ToObject<string>().Should().Be(venueAddressLine1);
                location["addressLine2"].ToObject<string>().Should().Be(venueAddressLine2);
                location["town"].ToObject<string>().Should().Be(venueTown);
                location["county"].ToObject<string>().Should().Be(venueCounty);
                location["postcode"].ToObject<string>().Should().Be(venuePostcode);
                location["telephone"].ToObject<string>().Should().Be(venueTelephone);
                location["email"].ToObject<string>().Should().Be(venueEmail);
                location["website"].ToObject<string>().Should().Be("http://" + venueWebsite);
                location["latitude"].ToObject<double>().Should().Be(venueLatitude);
                location["longitude"].ToObject<double>().Should().Be(venueLongitude);
            }

            static string HtmlEncode(string value) => System.Net.WebUtility.HtmlEncode(value);
        }
    }
}
