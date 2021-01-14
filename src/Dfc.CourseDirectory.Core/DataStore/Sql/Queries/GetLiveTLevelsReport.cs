using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLiveTLevelsReport : ISqlQuery<IAsyncEnumerable<LiveTLevelsReportItem>>
    {
    }

    public class LiveTLevelsReportItem
    {
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string TLevelName { get; set; }
        public string VenueName { get; set; }
        public DateTime StartDate { get; set; }
    }
}
