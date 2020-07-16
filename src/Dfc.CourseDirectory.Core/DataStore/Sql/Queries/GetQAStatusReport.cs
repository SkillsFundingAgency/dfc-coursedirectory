using Dfc.CourseDirectory.Core.Models;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetQAStatusReport : ISqlQuery<IReadOnlyCollection<GetQAStatusReportResult>>
    {
    }

    public class GetQAStatusReportResult
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string Email { get; set; }
        public DateTime? PassedQAOn { get; set; }
        public DateTime? FailedQAOn { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons? UnableToCompleteReasons { get; set; }
        public DateTime? UnabletoCompleteOn { get; set; }
        public string Notes { get; set; }
        public ApprenticeshipQAStatus? QAStatus { get; set; }
    }
}
