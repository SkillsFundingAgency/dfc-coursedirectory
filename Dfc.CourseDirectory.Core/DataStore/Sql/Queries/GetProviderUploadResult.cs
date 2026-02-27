using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderUploadResult : ISqlQuery<ProviderUploadResultSummary>
    {
        public Guid ProviderUploadId { get; set; }
    }
}
