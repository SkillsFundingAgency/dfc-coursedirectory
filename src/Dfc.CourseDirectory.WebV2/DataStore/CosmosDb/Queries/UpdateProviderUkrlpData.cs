using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class UpsertProviderUkrlpData : Provider, ICosmosDbQuery<Success>
    {
        public bool Update { get; set; }
    }
}
