using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData
{
    public class GetLiveCourseProvidersReport : ISqlQuery<IAsyncEnumerable<LiveCourseProvidersReportItem>>
    {
        public DateTime FromDate { get; set; }
    }

    public class LiveCourseProvidersReportItem
    {
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string TradingName { get; set; }
        public string ContactAddress1 { get; set; }
        public string ContactAddress2 { get; set; }
        public string AddressPostTown { get; set; }
        public string AddressCounty { get; set; }
        public string AddressPostcode { get; set; }
        public string Telephone { get; set; }
        public string WebsiteAddress { get; set; }
        public string Email { get; set; }
    }
}
