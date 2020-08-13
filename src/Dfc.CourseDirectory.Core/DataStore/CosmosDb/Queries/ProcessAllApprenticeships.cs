using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ProcessAllApprenticeships : ICosmosDbQuery<None>
    {
        public Func<IReadOnlyCollection<Apprenticeship>, Task> ProcessChunk { get; set; }
    }
}
