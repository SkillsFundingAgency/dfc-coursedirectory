using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetApprenticeshipById : ICosmosDbQuery<Apprenticeship>
    {
        public Guid ApprenticeshipId { get; set; }
    }
}
