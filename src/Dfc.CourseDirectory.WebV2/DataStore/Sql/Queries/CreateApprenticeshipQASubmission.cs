using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class CreateApprenticeshipQASubmission : ISqlQuery<int>
    {
        public Guid ProviderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string SubmittedByUserId { get; set; }
        public string ProviderBriefOverview { get; set; }
        public IEnumerable<Guid> ApprenticeshipIds { get; set; }
    }
}
