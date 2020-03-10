using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetProviderUkprnForApprenticeship : ICosmosDbQuery<OneOf<NotFound, int>>
    {
        public Guid ApprenticeshipId { get; set; }
    }
}
