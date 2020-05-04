using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipStatusHandler :
        ICosmosDbQueryHandler<UpdateApprenticeshipStatus, OneOf<NotFound, Success>>
    {
        public OneOf<NotFound, Success> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            UpdateApprenticeshipStatus request)
        {
            var app = inMemoryDocumentStore.Apprenticeships.All.SingleOrDefault(a => a.Id == request.ApprenticeshipId);

            if (app == null)
            {
                return new NotFound();
            }
            else
            {
                app.RecordStatus = request.Status;
                return new Success();
            }
        }
    }
}
