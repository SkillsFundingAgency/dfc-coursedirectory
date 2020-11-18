using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAProviderAssessmentForSubmission :
        ISqlQuery<ApprenticeshipQAProviderAssessment>
    {
        public int ApprenticeshipQASubmissionId { get; set; }
    }
}
