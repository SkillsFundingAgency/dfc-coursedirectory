using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ProcessAllVenues : ICosmosDbQuery<None>
    {
        public Func<IReadOnlyCollection<Venue>, Task> ProcessChunk { get; set; }
    }
}
