using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestUserSignInForProvider : ISqlQuery<OneOf<None, LatestUserSignInForProviderResult>>
    {
        public Guid ProviderId { get; set; }
    }

    public class LatestUserSignInForProviderResult
    {
        public UserInfo User { get; set; }
        public DateTime SignedInUtc { get; set; }
    }
}
