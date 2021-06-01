using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetVenueUploadProcessing : ISqlQuery<SetVenueUploadProcessingResult>
    {
        public Guid VenueUploadId { get; set; }
        public DateTime ProcessingStartedOn { get; set; }
    }

    public enum SetVenueUploadProcessingResult
    {
        Success = 0,
        NotFound = 1,
        AlreadyProcessed = 2
    }
}
