using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderBulkUploadStatus : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }
        public OneOf<None, bool> PublishInProgress { get; set; }
        public OneOf<None, bool> InProgress { get; set; }
        public OneOf<None, int> TotalRowCount { get; set; }
        public OneOf<None, DateTime> StartedTimestamp { get; set; }
    }
}
