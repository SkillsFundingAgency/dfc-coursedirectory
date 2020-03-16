using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests
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
