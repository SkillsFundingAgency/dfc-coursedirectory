using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task UpdateApprenticeshipQASubmission(
            int apprenticeshipQASubmissionId,
            bool? providerAssessmentPassed,
            bool? apprenticeshipAssessmentsPassed,
            bool? passed,
            string lastAssessedByUserId,
            DateTime lastAssessedOn) => WithSqlQueryDispatcher(
            dispatcher => dispatcher.ExecuteQuery(
                new UpdateApprenticeshipQASubmission()
                {
                    ApprenticeshipAssessmentsPassed = apprenticeshipAssessmentsPassed,
                    ApprenticeshipQASubmissionId = apprenticeshipQASubmissionId,
                    LastAssessedByUserId = lastAssessedByUserId,
                    LastAssessedOn = lastAssessedOn,
                    Passed = passed,
                    ProviderAssessmentPassed = providerAssessmentPassed
                }));
    }
}
