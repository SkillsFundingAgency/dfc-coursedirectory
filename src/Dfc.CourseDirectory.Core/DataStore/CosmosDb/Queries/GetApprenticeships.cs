using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetApprenticeships : ICosmosDbQuery<IDictionary<Guid, Apprenticeship>>
    {
        public GetApprenticeships(Expression<Func<Apprenticeship, bool>> predicate = null)
        {
            Predicate = predicate;
        }

        public Expression<Func<Apprenticeship, bool>> Predicate { get; set; }
    }
}
