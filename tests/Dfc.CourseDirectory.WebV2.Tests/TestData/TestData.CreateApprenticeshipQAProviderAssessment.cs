using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public Task CreateApprenticeshipQAProviderAssessment(
            int apprenticeshipQASubmissionId,
            string assessedByUserId,
            DateTime assessedOn,
            bool compliancePassed,
            string complianceComments,
            ApprenticeshipQAProviderComplianceFailedReasons complianceFailedReasons,
            bool stylePassed,
            string styleComments,
            ApprenticeshipQAProviderStyleFailedReasons styleFailedReasons
        ) => WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
            new CreateApprenticeshipQAProviderAssessment()
            {
                ApprenticeshipQASubmissionId = apprenticeshipQASubmissionId,
                AssessedByUserId = assessedByUserId,
                AssessedOn = assessedOn,
                CompliancePassed = compliancePassed,
                ComplianceComments = complianceComments,
                ComplianceFailedReasons = complianceFailedReasons,
                Passed = compliancePassed && stylePassed,
                StylePassed = stylePassed,
                StyleComments = styleComments,
                StyleFailedReasons = styleFailedReasons
            }));
    }
}
