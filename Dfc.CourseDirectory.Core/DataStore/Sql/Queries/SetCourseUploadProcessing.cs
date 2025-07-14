using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetCourseUploadProcessing : ISqlQuery<SetCourseUploadProcessingResult>
    {
        public Guid CourseUploadId { get; set; }
        public DateTime ProcessingStartedOn { get; set; }
    }

    public enum SetCourseUploadProcessingResult
    {
        Success = 0,
        NotFound = 1,
        AlreadyProcessed = 2
    }
}
