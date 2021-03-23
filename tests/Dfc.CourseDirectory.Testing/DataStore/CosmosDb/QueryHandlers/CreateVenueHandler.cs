using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class CreateVenueHandler : ICosmosDbQueryHandler<CreateVenue, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateVenue request)
        {
            var venue = new Venue()
            {
                Id = request.VenueId,
                Status = Core.Models.VenueStatus.Live,
                Ukprn = request.ProviderUkprn,
                VenueName = request.Name,
                Email = request.Email,
                PHONE = request.Telephone,
                Website = request.Website,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                Town = request.Town,
                County = request.County,
                Postcode = request.Postcode,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };
            inMemoryDocumentStore.Venues.Save(venue);

            return new Success();
    }
}
}
