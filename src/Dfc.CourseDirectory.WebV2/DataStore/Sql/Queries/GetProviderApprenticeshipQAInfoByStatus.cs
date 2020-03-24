using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetProviderApprenticeshipQAInfoByStatus
        : ISqlQuery<IReadOnlyCollection<GetProviderApprenticeshipQAInfoByStatusResult>>
    {
        public IEnumerable<ApprenticeshipQAStatus> Statuses { get; set; }
    }

    public class GetProviderApprenticeshipQAInfoByStatusResult
    {
        public Guid ProviderId { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public UserInfo LastAssessedBy { get; set; }
    }
}
