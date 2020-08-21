using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetApprenticeshipsByIdsHandler
        : ICosmosDbQueryHandler<GetApprenticeshipsByIds, IDictionary<Guid, Apprenticeship>>
    {
        public IDictionary<Guid, Apprenticeship> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetApprenticeshipsByIds request) =>
            inMemoryDocumentStore.Apprenticeships.All
                .Where(p => request.ApprenticeshipIds.Contains(p.Id))
                .Where(p => p.RecordStatus != 4 && p.RecordStatus != 8)
                .ToDictionary(p => p.Id, p => p);
    }
}
