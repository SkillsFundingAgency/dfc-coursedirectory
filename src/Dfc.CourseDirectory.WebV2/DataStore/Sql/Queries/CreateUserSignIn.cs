using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class CreateUserSignIn : ISqlQuery<None>
    {
        public UserInfo User { get; set; }
        public DateTime SignedInUtc { get; set; }
        public Guid? CurrentProviderId { get; set; }
    }
}
