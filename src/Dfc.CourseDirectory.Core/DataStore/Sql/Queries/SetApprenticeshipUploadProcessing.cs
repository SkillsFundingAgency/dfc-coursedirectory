using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetApprenticeshipUploadProcessing : ISqlQuery<SetApprenticeshipUploadProcessingResult>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public DateTime ProcessingStartedOn { get; set; }
    }

    public enum SetApprenticeshipUploadProcessingResult
    {
        Success = 0,
        NotFound = 1,
        AlreadyProcessed = 2
    }
}
