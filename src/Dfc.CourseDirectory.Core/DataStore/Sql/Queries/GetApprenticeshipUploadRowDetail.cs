using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetApprenticeshipUploadRowDetail : ISqlQuery<ApprenticeshipUploadRow>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public int RowNumber { get; set; }
    }
}
