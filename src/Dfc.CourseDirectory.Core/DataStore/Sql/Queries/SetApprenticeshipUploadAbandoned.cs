using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetApprenticeshipUploadAbandoned : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public DateTime AbandonedOn { get; set; }
    }
}
