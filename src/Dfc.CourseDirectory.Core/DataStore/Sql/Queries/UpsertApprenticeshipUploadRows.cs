using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertApprenticeshipUploadRows : ISqlQuery<IReadOnlyCollection<ApprenticeshipUploadRow>>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime ValidatedOn { get; set; }
        public IEnumerable<UpsertApprenticeshipUploadRowsRecord> Records { get; set; }
    }

    public class UpsertApprenticeshipUploadRowsRecord
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
