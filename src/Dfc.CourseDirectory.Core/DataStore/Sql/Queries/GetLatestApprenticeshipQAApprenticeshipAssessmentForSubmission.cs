using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission :
        ISqlQuery<ApprenticeshipQAApprenticeshipAssessment>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ApprenticeshipQASubmissionId { get; set; }
    }
}
