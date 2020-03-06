using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAProviderAssessmentForSubmission
        : ISqlQuery<OneOf<None, ApprenticeshipQAProviderAssessment>>
    {
        public int ApprenticeshipQASubmissionId { get; set; }
    }
}
