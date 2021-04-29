using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderVenuesLastUpdated : ISqlQuery<DateTime?>
    {
        public Guid ProviderId { get; set; }
    }
}
