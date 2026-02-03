using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderUploadProcessed : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderUploadId { get; set; }
        public DateTime ProcessingCompletedOn { get; set; }
        public bool IsValid { get; set; }
    }
}
