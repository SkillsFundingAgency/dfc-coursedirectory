using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Functions.Tests.Builders
{
    public class VenueBuilder
    {
        private decimal? _latitude;
        private decimal? _longitude;

        public Venue Build()
        {
            var town = Faker.Address.City();
            return
                new Venue
                {
                    Id = Guid.NewGuid(),
                    VenueName = $"{town} venue",
                    Ukprn = Faker.RandomNumber.Next(),
                    AddressLine1 = Faker.Address.StreetAddress(),
                    AddressLine2 = Faker.Address.SecondaryAddress(),
                    Town = town,
                    County = Faker.Address.UkCounty(),
                    Postcode = Faker.Address.UkPostCode().ToUpper(),
                    Latitude = _latitude ?? DfcFaker.UkLatitude(),
                    Longitude = _longitude ?? DfcFaker.UkLongitude(),
                    PHONE = Faker.Phone.Number(),
                    Email = Faker.Internet.Email(),
                    Website = Faker.Internet.Url(),
                    Status = (int)VenueStatus.Live,
                };
        }

        public VenueBuilder WithLatitude(decimal latitude)
        {
            _latitude = latitude;
            return this;
        }

        public VenueBuilder WithLongitude(decimal longitude)
        {
            _longitude = longitude;
            return this;
        }
    }
}
