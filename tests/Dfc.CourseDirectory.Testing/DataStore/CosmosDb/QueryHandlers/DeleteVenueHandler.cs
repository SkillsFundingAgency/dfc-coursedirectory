using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class DeleteVenueHandler : ICosmosDbQueryHandler<DeleteVenue, OneOf<NotFound, Success>>
    {
        public OneOf<NotFound, Success> Execute(InMemoryDocumentStore inMemoryDocumentStore, DeleteVenue request)
        {
            var venue = inMemoryDocumentStore.Venues.All.FirstOrDefault(v => v.Id == request.VenueId);

            if (venue == null)
            {
                return new NotFound();
            }

            venue.Status = (int)VenueStatus.Archived;
            venue.DateUpdated = request.UpdatedDate;
            venue.UpdatedBy = request.UpdatedBy.UserId;

            inMemoryDocumentStore.Venues.Save(venue);

            return new Success();
        }
    }
}
