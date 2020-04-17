using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
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
        public DateTime AddedOn { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons? UnableToCompleteReasons { get; set; }
    }
}
