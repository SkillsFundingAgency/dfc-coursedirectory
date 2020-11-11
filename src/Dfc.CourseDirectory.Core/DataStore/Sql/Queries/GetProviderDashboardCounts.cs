using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderDashboardCounts : ISqlQuery<(int CourseCount, int ApprenticeshipCount, int VenueCount)>
    {
        public Guid ProviderId { get; set; }
    }
}
