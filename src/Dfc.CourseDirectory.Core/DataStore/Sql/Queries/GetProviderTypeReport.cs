using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderTypeReport : ISqlQuery<IAsyncEnumerable<ProviderTypeReportItem>>
    {
    }

    public class ProviderTypeReportItem
    {
        public int ProviderUkprn { get; set; }
        public string ProviderName { get; set; }
        public ProviderType ProviderType { get; set; }
        public ProviderStatus ProviderStatus { get; set; }
        public string UkrlpProviderStatusDescription { get; set; }
        public int LiveCourseCount { get; set; }
        public int OtherCourseCount { get; set; }
        public int LiveApprenticeshipCount { get; set; }
        public int OtherApprenticeshipCount { get; set; }
        public int LiveTLevelCount { get; set; }
        public int OtherTLevelCount { get; set; }
    }
}
