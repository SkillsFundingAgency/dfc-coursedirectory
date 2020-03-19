using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetAllStandards : ICosmosDbQuery<IReadOnlyCollection<Standard>>
    {
    }
}
