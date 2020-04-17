using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateApprenticeshipQASubmission : ISqlQuery<int>
    {
        public Guid ProviderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string SubmittedByUserId { get; set; }
        public string ProviderMarketingInformation { get; set; }
        public IEnumerable<CreateApprenticeshipQASubmissionApprenticeship> Apprenticeships { get; set; }
    }

    public class CreateApprenticeshipQASubmissionApprenticeship
    {
        public Guid ApprenticeshipId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
    }
}
