using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue
{
    [JourneyState]
    public class EditVenueJourneyModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool NewAddressIsOutsideOfEngland { get; set; }
    }

    public class EditVenueJourneyModelFactory
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public EditVenueJourneyModelFactory(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<EditVenueJourneyModel> CreateModel(Guid venueId)
        {
            var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = venueId });

            if (venue == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Venue, venueId);
            }

            return new EditVenueJourneyModel()
            {
                Email = venue.Email,
                Name = venue.VenueName,
                PhoneNumber = venue.Telephone,
                Website = venue.Website,
                AddressLine1 = venue.AddressLine1,
                AddressLine2 = venue.AddressLine2,
                Town = venue.Town,
                County = venue.County,
                Postcode = venue.Postcode,
                Latitude = Convert.ToDouble(venue.Latitude),
                Longitude = Convert.ToDouble(venue.Longitude)
            };
        }
    }
}
