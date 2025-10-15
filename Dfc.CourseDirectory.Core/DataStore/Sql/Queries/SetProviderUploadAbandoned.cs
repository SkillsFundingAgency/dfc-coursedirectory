using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderUploadAbandoned : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderUploadId { get; set; }
        public DateTime AbandonedOn { get; set; }
    }
}
