using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderUploadProcessing : ISqlQuery<SetProviderUploadProcessingResult>
    {
        public Guid ProviderUploadId { get; set; }
        public DateTime ProcessingStartedOn { get; set; }
    }

    public enum SetProviderUploadProcessingResult
    {
        Success = 0,
        NotFound = 1,
        AlreadyProcessed = 2
    }
}
