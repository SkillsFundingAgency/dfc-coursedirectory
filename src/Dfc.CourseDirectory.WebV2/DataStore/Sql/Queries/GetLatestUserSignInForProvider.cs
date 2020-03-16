using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
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
