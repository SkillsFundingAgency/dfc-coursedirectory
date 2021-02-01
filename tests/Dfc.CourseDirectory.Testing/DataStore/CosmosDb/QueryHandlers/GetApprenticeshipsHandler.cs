using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetApprenticeshipsHandler : ICosmosDbQueryHandler<GetApprenticeships, IDictionary<Guid, Apprenticeship>>
    {
        public IDictionary<Guid, Apprenticeship> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetApprenticeships request)
        {
            var query = inMemoryDocumentStore.Apprenticeships.All.AsQueryable();

            if (request.Predicate != null)
            {
                query = query.Where(request.Predicate);
            }

            return query.ToDictionary(a => a.Id, a => a);
        }
    }
}
