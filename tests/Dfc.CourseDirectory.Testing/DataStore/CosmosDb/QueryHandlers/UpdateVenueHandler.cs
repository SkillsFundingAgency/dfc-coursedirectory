using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateVenueHandler : ICosmosDbQueryHandler<UpdateVenue, OneOf<NotFound, Success>>
    {
        public OneOf<NotFound, Success> Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateVenue request)
        {
            var venue = inMemoryDocumentStore.Venues.All.SingleOrDefault(venue => venue.Id == request.VenueId);

            if (venue == null)
            {
                return new NotFound();
            }

            venue.VenueName = request.Name;
            venue.Email = request.Email;
            venue.Telephone = request.PhoneNumber;
            venue.Website = request.Website;
            venue.AddressLine1 = request.AddressLine1;
            venue.AddressLine2 = request.AddressLine2;
            venue.Town = request.Town;
            venue.County = request.County;
            venue.Postcode = request.Postcode;
            venue.Latitude = request.Latitude;
            venue.Longitude = request.Longitude;

            inMemoryDocumentStore.Venues.Save(venue);

            return new Success();
        }
    }
}
