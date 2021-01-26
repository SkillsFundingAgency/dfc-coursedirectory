using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ProcessAllApprenticeships : ICosmosDbQuery<None>
    {
        public Expression<Func<Apprenticeship, bool>> Predicate { get; set; }
        public int? MaxBatchSize { get; set; }
        public Func<IReadOnlyCollection<Apprenticeship>, Task> ProcessChunk { get; set; }

        public ProcessAllApprenticeships(Expression<Func<Apprenticeship, bool>> predicate = null, int? maxBatchSize = null)
        {
            Predicate = predicate;
            MaxBatchSize = maxBatchSize;
        }
    }
}
