using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderOnboarded : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }

        public UserInfo UpdatedBy { get; set; }

        public DateTime UpdatedDateTime { get; set; }
    }
}
