using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAProviderAssessmentForSubmission
        : ISqlQuery<OneOf<None, ApprenticeshipQAProviderAssessment>>
    {
        public int ApprenticeshipQASubmissionId { get; set; }
    }
}
