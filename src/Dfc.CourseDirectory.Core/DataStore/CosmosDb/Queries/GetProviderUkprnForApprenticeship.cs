using System;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetProviderUkprnForApprenticeship : ICosmosDbQuery<int?>
    {
        public Guid ApprenticeshipId { get; set; }
    }
}
