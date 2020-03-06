using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class GetLatestApprenticeshipQAProviderAssessmentForSubmissionHandler
        : ISqlQueryHandler<GetLatestApprenticeshipQAProviderAssessmentForSubmission, OneOf<None, ApprenticeshipQAProviderAssessment>>
    {
        public async Task<OneOf<None, ApprenticeshipQAProviderAssessment>> Execute(
            SqlTransaction transaction,
            GetLatestApprenticeshipQAProviderAssessmentForSubmission query)
        {
            var sql = @"
SELECT TOP 1
    s.ApprenticeshipQASubmissionId,
    s.AssessedOn,
    s.Passed,
    s.CompliancePassed,
    s.ComplianceFailedReasons,
    s.ComplianceComments,
    s.StylePassed,
    s.StyleFailedReasons,
    s.StyleComments,
    u.UserId,
    u.Email,
    u.FirstName,
    u.LastName
FROM Pttcd.ApprenticeshipQASubmissionProviderAssessments s
LEFT JOIN Pttcd.Users u ON s.AssessedByUserId = u.UserId
WHERE s.ApprenticeshipQASubmissionId = @ApprenticeshipQASubmissionId
ORDER BY s.AssessedOn DESC";

            var paramz = new { query.ApprenticeshipQASubmissionId };

            var result = (await transaction.Connection.QueryAsync<ApprenticeshipQAProviderAssessment, UserInfo, ApprenticeshipQAProviderAssessment>(
                sql,
                (r, assessedBy) =>
                {
                    r.AssessedBy = assessedBy;
                    return r;
                },
                paramz,
                transaction,
                splitOn: "UserId")).SingleOrDefault();

            if (result != null)
            {
                return result;
            }
            else
            {
                return new None();
            }
        }
    }
}
