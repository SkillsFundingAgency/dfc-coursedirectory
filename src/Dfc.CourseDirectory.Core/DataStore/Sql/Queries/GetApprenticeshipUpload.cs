using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetApprenticeshipUpload : ISqlQuery<ApprenticeshipUpload>
    {
        public Guid ApprenticeshipUploadId { get; set; }
    }
}
