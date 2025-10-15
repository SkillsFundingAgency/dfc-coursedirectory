using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderOfferingsLastUpdated : ISqlQuery<DateTime?>
    {
        public Guid ProviderId { get; set; }
    }
}
