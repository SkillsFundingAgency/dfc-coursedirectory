using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestUserSignInForProvider : ISqlQuery<LatestUserSignInForProviderResult>
    {
        public Guid ProviderId { get; set; }
    }

    public class LatestUserSignInForProviderResult
    {
        public UserInfo User { get; set; }
        public DateTime SignedInUtc { get; set; }
    }
}
