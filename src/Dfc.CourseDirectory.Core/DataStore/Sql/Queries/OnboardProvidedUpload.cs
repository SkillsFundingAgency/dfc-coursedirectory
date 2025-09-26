using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class OnboardProvidedUpload : ISqlQuery<OneOf<NotFound, OnboardProviderUploadResult>>
    {
        public Guid ProviderUploadId { get; set; }
        public UserInfo OnboardedBy { get; set; }
        public DateTime OnboardedOn { get; set; }
    }
}
