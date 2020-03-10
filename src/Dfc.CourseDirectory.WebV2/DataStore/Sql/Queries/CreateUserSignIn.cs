using System;
using Dfc.CourseDirectory.WebV2.Security;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class CreateUserSignIn : ISqlQuery<None>
    {
        public AuthenticatedUserInfo User { get; set; }
        public DateTime SignedInUtc { get; set; }
    }
}
