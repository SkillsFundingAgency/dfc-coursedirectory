using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetApprenticeshipsByIdsHandler
        : ICosmosDbQueryHandler<GetApprenticeshipsByIds, IDictionary<Guid, Apprenticeship>>
    {
        public IDictionary<Guid, Apprenticeship> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetApprenticeshipsByIds request) =>
            inMemoryDocumentStore.Apprenticeships.All
                .Where(p => request.ApprenticeshipIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);
    }
}
