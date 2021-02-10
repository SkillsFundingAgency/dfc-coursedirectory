using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderOnboarded : ICosmosDbQuery<OneOf<NotFound, AlreadyOnboarded, Success>>
    {
        public Guid ProviderId { get; set; }

        public UserInfo UpdatedBy { get; set; }

        public DateTime UpdatedDateTime { get; set; }
    }

    public class AlreadyOnboarded
    {
    }
}
