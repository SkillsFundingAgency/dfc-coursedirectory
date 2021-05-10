using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestCourseUploadForProviderWithStatus : ISqlQuery<CourseUpload>
    {
        public Guid ProviderId { get; set; }
        public IEnumerable<UploadStatus> Statuses { get; set; }
    }
}
