using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Functions.FixVenues;
using Dfc.CourseDirectory.Functions.Tests.Builders;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Functions.Tests.VenueFixup
{
    public class VenueCorrectionFinderTests
    {
        [MemberData(nameof(VenueTestCases))]
        [Theory]
        public void FindsCorrectVenues(VenueTestCase venueTestCase)
        {
            var corruptVenueIds = new List<Guid?>
            {
                null,
                Guid.Empty,
                new Guid("7A518C84-77F7-4D29-B579-DA08C4B0EDA4"),
                new Guid("3FBDE15F-3E40-498B-AAA0-A3B8A7225B5D"),
            };

            var potentialMatch = new VenueBuilder()
                .WithLatitude(60) // fixed value to avoid randomly matching
                .WithLongitude(-1) // fixed value to avoid randomly matching
                .Build();
            var locationVenueName = venueTestCase.MatchingName ? potentialMatch.VenueName : "no-match";
            var locationAddress = new ApprenticeshipLocationAddress
            {
                Address1 = venueTestCase.MatchingAddress1 ? potentialMatch.AddressLine1 : "no-match",
                Address2 = venueTestCase.MatchingAddress2 ? potentialMatch.AddressLine2 : "no-match",
                Town = venueTestCase.MatchingTown ? potentialMatch.Town : "no-match",
                County = venueTestCase.MatchingCounty ? potentialMatch.County : "no-match",
                Postcode = venueTestCase.MatchingPostcode ? potentialMatch.Postcode : "no-match",
                Latitude = venueTestCase.MatchingLatLong ? potentialMatch.Latitude : 50,
                Longitude = venueTestCase.MatchingLatLong ? potentialMatch.Longitude : 2,
                Email = venueTestCase.MatchingEmail ? potentialMatch.Email : "no-match",
                Phone = venueTestCase.MatchingPhone ? potentialMatch.PHONE : "no-match",
                Website = venueTestCase.MatchingWebsite ? potentialMatch.Website : "no-match",
            };

            foreach (var corruptVenueId in corruptVenueIds)
            {
                TestVenueMatch(locationVenueName, locationAddress, corruptVenueId, potentialMatch, venueTestCase.ShouldReturnVenueCorrection, venueTestCase.Reason);
            }
        }

        [Fact]
        public void FindsMatchingDuplicateVenues() // seen in dev data
        {
            var potentialMatch = new VenueBuilder()
                .WithLatitude(60) // fixed value to avoid randomly matching
                .WithLongitude(-1) // fixed value to avoid randomly matching
                .Build();

            var duplicateMatch = VenueCloner.CloneVenue(potentialMatch);

            var availableVenues = new List<Venue>
            {
                new VenueBuilder().Build(), // decoy venue to make sure it doesn't pick the wrong one
                potentialMatch,
                duplicateMatch,
            };

            var locationVenueName = potentialMatch.VenueName;
            var locationAddress = new ApprenticeshipLocationAddress
            {
                Address1 = potentialMatch.AddressLine1,
                Address2 = potentialMatch.AddressLine2,
                Town = potentialMatch.Town,
                County = potentialMatch.County,
                Postcode = potentialMatch.Postcode,
                Latitude = potentialMatch.Latitude,
                Longitude = potentialMatch.Longitude,
                Email = potentialMatch.Email,
                Phone = potentialMatch.PHONE,
                Website = potentialMatch.Website,
            };
            var location = new ApprenticeshipLocation
            {
                VenueId = Guid.Empty,
                RecordStatus = (int)ApprenticeshipStatus.Live,
                Name = locationVenueName,
                Address = locationAddress,
            };

            // act
            var matchingVenues = new VenueCorrectionFinder().GetMatchingVenues(location, availableVenues);
            matchingVenues.Should().BeEquivalentTo(duplicateMatch, potentialMatch);
        }

        public static IEnumerable<object[]> VenueTestCases()
        {
            return new []
            {
                // There are 2^10 combinations, so just testing interesting edge cases rather than all of them
                new VenueTestCase(
                    matchingName:true,
                    matchingAddress1:true, matchingAddress2:true, matchingTown:true, matchingCounty:true, matchingPostcode:true,
                    matchingLatLong:true,
                    matchingEmail:true, matchingPhone:true, matchingWebsite:true,
                    shouldReturnVenueCorrection: true, "complete match"),
                new VenueTestCase(
                    matchingName: true,
                    matchingAddress1: false, matchingAddress2: true, matchingTown: true, matchingCounty: true, matchingPostcode: true,
                    matchingLatLong: true,
                    matchingEmail: true, matchingPhone: true, matchingWebsite: true,
                    shouldReturnVenueCorrection: true, "match on name"),
                new VenueTestCase(
                    matchingName: false,
                    matchingAddress1: true, matchingAddress2: true, matchingTown: true, matchingCounty: true, matchingPostcode: true,
                    matchingLatLong: true,
                    matchingEmail: true, matchingPhone: true, matchingWebsite: true,
                    shouldReturnVenueCorrection: true, "match on address"),
                new VenueTestCase(
                    matchingName: true,
                    matchingAddress1: true, matchingAddress2: true, matchingTown: true, matchingCounty: true, matchingPostcode: true,
                    matchingLatLong: true,
                    matchingEmail: true, matchingPhone: false, matchingWebsite: false,
                    shouldReturnVenueCorrection: false, "not enough matching fields"),
                new VenueTestCase(
                    matchingName: false,
                    matchingAddress1: false, matchingAddress2: false, matchingTown: false, matchingCounty: false, matchingPostcode: false,
                    matchingLatLong: false,
                    matchingEmail: false, matchingPhone: false, matchingWebsite: false,
                    shouldReturnVenueCorrection: false, "no matching fields"),
            }.Select(testCase => new []{testCase});
        }

        private static void TestVenueMatch(string locationVenueName, ApprenticeshipLocationAddress locationAddress,
            Guid? originalVenueGuid, Venue potentialMatch, bool expectVenueCorrection, string reason)
        {
            var availableVenues = new List<Venue>
            {
                new VenueBuilder().Build(), // decoy venue to make sure it doesn't pick the wrong one
            };

            if (potentialMatch != null)
            {
                availableVenues.Add(potentialMatch);
            }

            availableVenues.Add(new VenueBuilder().Build()); // another decoy venue to make sure it doesn't pick the wrong one

            var location = new ApprenticeshipLocation
            {
                VenueId = originalVenueGuid,
                RecordStatus = (int)ApprenticeshipStatus.Live,
                Name = locationVenueName,
                Address = locationAddress,
            };

            // act
            var matchingVenues = new VenueCorrectionFinder().GetMatchingVenues(location, availableVenues);

            // assert
            if (expectVenueCorrection)
            {
                var matchingVenue = matchingVenues.Should().ContainSingle().Subject;
                matchingVenue.Should().Be(potentialMatch, reason);
            }
            else
            {
                matchingVenues.Should().BeEmpty(reason);
            }
        }
    }

    public class VenueTestCase
    {
        public bool MatchingName{get;}
        public bool MatchingAddress1{get;}
        public bool MatchingAddress2{get;}
        public bool MatchingTown{get;}
        public bool MatchingCounty{get;}
        public bool MatchingPostcode{get;}
        public bool MatchingLatLong{get;}
        public bool MatchingEmail{get;}
        public bool MatchingPhone{get;}
        public bool MatchingWebsite{get;}
        public bool ShouldReturnVenueCorrection{get;}
        public string Reason{get;}

        public VenueTestCase(bool matchingName,
            bool matchingAddress1,
            bool matchingAddress2,
            bool matchingTown,
            bool matchingCounty,
            bool matchingPostcode,
            bool matchingLatLong,
            bool matchingEmail,
            bool matchingPhone,
            bool matchingWebsite,
            bool shouldReturnVenueCorrection,
            string reason)
        {
            MatchingName = matchingName;
            MatchingAddress1 = matchingAddress1;
            MatchingAddress2 = matchingAddress2;
            MatchingTown = matchingTown;
            MatchingCounty = matchingCounty;
            MatchingPostcode = matchingPostcode;
            MatchingLatLong = matchingLatLong;
            MatchingEmail = matchingEmail;
            MatchingPhone = matchingPhone;
            MatchingWebsite = matchingWebsite;
            ShouldReturnVenueCorrection = shouldReturnVenueCorrection;
            Reason = reason;
        }

        public override string ToString()
        {
            var fields = new List<string>();
            if (MatchingName)     fields.Add(nameof(MatchingName));
            if (MatchingAddress1) fields.Add(nameof(MatchingAddress1));
            if (MatchingAddress2) fields.Add(nameof(MatchingAddress2));
            if (MatchingTown)     fields.Add(nameof(MatchingTown));
            if (MatchingCounty)   fields.Add(nameof(MatchingCounty));
            if (MatchingPostcode) fields.Add(nameof(MatchingPostcode));
            if (MatchingLatLong)  fields.Add(nameof(MatchingLatLong));
            if (MatchingEmail)    fields.Add(nameof(MatchingEmail));
            if (MatchingPhone)    fields.Add(nameof(MatchingPhone));
            if (MatchingWebsite)  fields.Add(nameof(MatchingWebsite));
            return (fields.IsEmpty() ? "NothingMatching" : "")
                   + string.Join(',', fields)
                   + " => "
                   + (ShouldReturnVenueCorrection ? "Updates Venue" : "Venue Unchanged");
        }
    }
}
