using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class DeleteApprenticeship : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ProviderUkprn { get; set; }
    }
}
