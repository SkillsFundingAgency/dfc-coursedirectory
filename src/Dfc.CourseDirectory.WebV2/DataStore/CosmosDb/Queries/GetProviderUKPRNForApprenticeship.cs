using System;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetProviderUKPRNForApprenticeship : ICosmosDbQuery<int?>
    {
        public Guid ApprenticeshipId { get; set; }
    }
}
