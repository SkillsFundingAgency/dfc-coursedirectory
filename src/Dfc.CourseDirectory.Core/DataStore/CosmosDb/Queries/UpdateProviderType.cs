using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderType : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }
        public ProviderType ProviderType { get; set; }
    }
}
