using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderUploadInvalidRowCount : ISqlQuery<int>
    {
        public Guid ProviderUploadId { get; set; }
    }
}
