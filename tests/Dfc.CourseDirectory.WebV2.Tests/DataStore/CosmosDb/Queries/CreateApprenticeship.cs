using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries
{
    public class CreateApprenticeship : ICosmosDbQuery<CreateApprenticeshipStatus>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ProviderUkprn { get; set; }
    }

    public enum CreateApprenticeshipStatus { Ok }
}
