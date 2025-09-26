using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderUpload : ISqlQuery<CourseUpload>
    {
        public Guid ProviderUploadId { get; set; }
    }
}
