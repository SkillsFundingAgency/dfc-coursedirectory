using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Functions.Tests.VenueFixup
{
    public static class VenueCloner
    {
        public static Venue CloneVenue(Venue source)
        {
            return new Venue
            {
                Id = Guid.NewGuid(),
                Status = (int)VenueStatus.Live,
                VenueName = source.VenueName,
                Ukprn = source.Ukprn,
                AddressLine1 = source.AddressLine1,
                AddressLine2 = source.AddressLine2,
                Town = source.Town,
                County = source.County,
                Postcode = source.Postcode,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                PHONE = source.PHONE,
                Email = source.Email,
                Website = source.Website,
            };
        }
    }
}
