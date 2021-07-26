using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetApprenticeshipUploadInvalidRowCount : ISqlQuery<int>
    {
        public Guid ApprenticeshipUploadId { get; set; }
    }
}
