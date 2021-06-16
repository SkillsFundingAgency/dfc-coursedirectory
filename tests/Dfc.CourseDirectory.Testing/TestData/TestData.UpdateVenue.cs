using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<Venue> UpdateVenue(
                Guid venueId,
                UserInfo updatedBy,
                Action<Venue, UpdateVenue> updateCommand = null) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                var venue = await dispatcher.ExecuteQuery(new GetVenue() { VenueId = venueId });
                Assert.NotNull(venue);

                var command = new UpdateVenue()
                {
                    AddressLine1 = venue.AddressLine1,
                    AddressLine2 = venue.AddressLine2,
                    County = venue.County,
                    Email = venue.Email,
                    Latitude = venue.Latitude,
                    Longitude = venue.Longitude,
                    Name = venue.VenueName,
                    Postcode = venue.Postcode,
                    Telephone = venue.Telephone,
                    Town = venue.Town,
                    UpdatedBy = updatedBy,
                    UpdatedOn = _clock.UtcNow,
                    VenueId = venueId,
                    Website = venue.Website
                };

                updateCommand?.Invoke(venue, command);

                await dispatcher.ExecuteQuery(command);

                venue = await dispatcher.ExecuteQuery(new GetVenue() { VenueId = venueId });
                return venue;
            });
    }
}
