using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderBulkUploadStatus : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }

        public bool PublishInProgress { get; set; }
    }
}
