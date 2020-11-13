using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderDashboardCounts : ISqlQuery<(int CourseRunCount, int ApprenticeshipCount, int VenueCount)>
    {
        public Guid ProviderId { get; set; }
    }
}
