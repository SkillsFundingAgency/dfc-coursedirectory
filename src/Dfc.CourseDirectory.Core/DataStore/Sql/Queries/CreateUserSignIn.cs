using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateUserSignIn : ISqlQuery<None>
    {
        public UserInfo User { get; set; }
        public DateTime SignedInUtc { get; set; }
        public Guid? CurrentProviderId { get; set; }
    }
}
