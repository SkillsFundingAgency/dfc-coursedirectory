using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class ArchiveApprenticeshipsForProviderHandler : ICosmosDbQueryHandler<ArchiveApprenticeshipsForProvider, int>
    {
        public int Execute(InMemoryDocumentStore inMemoryDocumentStore, ArchiveApprenticeshipsForProvider request)
        {
            var providerApprenticeships = inMemoryDocumentStore.Apprenticeships.All
                .Where(a => a.ProviderUKPRN == request.Ukprn && a.RecordStatus != 4);

            var updated = 0;

            foreach (var app in providerApprenticeships)
            {
                app.RecordStatus = (int)ApprenticeshipStatus.Archived;

                foreach (var location in app.ApprenticeshipLocations)
                {
                    location.RecordStatus = 4;
                }

                inMemoryDocumentStore.Apprenticeships.Save(app);

                updated++;
            }

            return updated;
        }
    }
}
