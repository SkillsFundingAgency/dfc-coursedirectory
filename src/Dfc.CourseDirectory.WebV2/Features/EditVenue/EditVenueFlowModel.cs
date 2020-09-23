using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue
{
    [FormFlowState]
    public class EditVenueFlowModel
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
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool NewAddressIsOutsideOfEngland { get; set; }
    }

    public class EditVenueFlowModelFactory
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public EditVenueFlowModelFactory(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<EditVenueFlowModel> CreateModel(Guid venueId)
        {
            var venue = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenueById() { VenueId = venueId });

            if (venue == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Venue, venueId);
            }

            return new EditVenueFlowModel()
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
                Latitude = venue.Latitude,
                Longitude = venue.Longitude
            };
        }
    }
}
