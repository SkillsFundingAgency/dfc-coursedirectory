using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateApprenticeshipUpload : ISqlQuery<Success>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime CreatedOn { get; set; }
        public UserInfo CreatedBy { get; set; }
    }
}
