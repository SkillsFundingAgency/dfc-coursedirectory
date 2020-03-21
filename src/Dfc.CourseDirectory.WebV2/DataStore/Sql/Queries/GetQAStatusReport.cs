using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetQAStatusReport : ISqlQuery<IReadOnlyCollection<GetQAStatusReportResult>>
    {
    }

    public class GetQAStatusReportResult
    {
        public int UKPRN { get; set; }
    }
}
