using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestUnpublishedApprenticeshipUploadForProvider : ISqlQuery<ApprenticeshipUpload>
    {
        public Guid ProviderId { get; set; }
    }
}
