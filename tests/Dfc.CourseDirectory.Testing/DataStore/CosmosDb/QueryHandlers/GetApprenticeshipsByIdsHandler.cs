using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetApprenticeshipsByIdsHandler
        : ICosmosDbQueryHandler<GetApprenticeshipsByIds, IDictionary<Guid, Apprenticeship>>
    {
        public IDictionary<Guid, Apprenticeship> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetApprenticeshipsByIds request) =>
            inMemoryDocumentStore.Apprenticeships.All
                .Where(p => request.ApprenticeshipIds.Contains(p.Id))
                .Where(p => p.RecordStatus != (int)ApprenticeshipStatus.Archived && p.RecordStatus != (int)ApprenticeshipStatus.Deleted)
                .ToDictionary(p => p.Id, p => p);
    }
}
