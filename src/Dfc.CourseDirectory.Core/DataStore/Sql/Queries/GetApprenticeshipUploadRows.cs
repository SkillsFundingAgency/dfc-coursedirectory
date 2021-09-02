using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetApprenticeshipUploadRows : ISqlQuery<(IReadOnlyCollection<ApprenticeshipUploadRow> Rows, int TotalRows)>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public bool WithErrorsOnly { get; set; }
    }
}
