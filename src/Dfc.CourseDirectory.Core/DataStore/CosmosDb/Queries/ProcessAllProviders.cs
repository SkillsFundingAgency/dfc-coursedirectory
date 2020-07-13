using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ProcessAllProviders : ICosmosDbQuery<None>
    {
        public Func<Provider, Task> Process { get; set; }
    }
}
