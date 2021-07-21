using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetApprenticeshipUploadProcessed : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public DateTime ProcessingCompletedOn { get; set; }
        public bool IsValid { get; set; }
    }
}
