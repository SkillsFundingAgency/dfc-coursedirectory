using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderOnboarded : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }

        public string UpdatedBy { get; set; }
    }
}
