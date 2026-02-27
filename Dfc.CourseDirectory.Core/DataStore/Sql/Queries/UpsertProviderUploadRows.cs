using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertProviderUploadRows : ISqlQuery<IReadOnlyCollection<ProviderUploadRow>>
    {
        public Guid ProviderUploadId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime ValidatedOn { get; set; }
        public IEnumerable<UpsertProviderUploadRowsRecord> Records { get; set; }

    }
    public class UpsertInactiveProviderUploadRows : ISqlQuery<IReadOnlyCollection<ProviderUploadRow>>
    {
        public Guid ProviderUploadId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime ValidatedOn { get; set; }
        public IEnumerable<UpsertInactiveProviderUploadRowsRecord> Records { get; set; }

    }
}
