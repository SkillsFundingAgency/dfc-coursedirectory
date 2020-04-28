using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateApprenticeshipStatus : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ProviderUkprn { get; set; }
        public int Status { get; set; }
    }
}
