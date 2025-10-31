using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class InactiveProviderUpload : ISqlQuery<OneOf<NotFound, OnboardProviderUploadResult>>
    {
        public Guid ProviderUploadId { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
