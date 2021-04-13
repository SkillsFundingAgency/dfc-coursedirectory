using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderMissingPrimaryContactReport : ISqlQuery<IAsyncEnumerable<ProviderMissingPrimaryContactReportItem>>
    {
    }

    public class ProviderMissingPrimaryContactReportItem
    {
        public int ProviderUkprn { get; set; }
        public string ProviderName { get; set; }
        public ProviderType ProviderType { get; set; }
        public ProviderStatus ProviderStatus { get; set; }
        public string UkrlpProviderStatusDescription { get; set; }
    }
}
