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
    public class GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmissionHandler
        : ISqlQueryHandler<GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission, OneOf<None, ApprenticeshipQAApprenticeshipAssessment>>
    {
        public async Task<OneOf<None, ApprenticeshipQAApprenticeshipAssessment>> Execute(
            SqlTransaction transaction,
            GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission query)
        {
            var sql = @"
SELECT TOP 1
    x.ApprenticeshipQASubmissionId,
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
FROM Pttcd.ApprenticeshipQASubmissionApprenticeshipAssessments s
JOIN Pttcd.ApprenticeshipQASubmissionApprenticeships x ON s.ApprenticeshipQASubmissionApprenticeshipId = x.ApprenticeshipQASubmissionApprenticeshipId
LEFT JOIN Pttcd.Users u ON s.AssessedByUserId = u.UserId
WHERE x.ApprenticeshipQASubmissionId = @ApprenticeshipQASubmissionId
AND x.ApprenticeshipId = @ApprenticeshipId
ORDER BY s.AssessedOn DESC";

            var paramz = new
            {
                query.ApprenticeshipQASubmissionId,
                query.ApprenticeshipId
            };

            var result = (await transaction.Connection.QueryAsync<ApprenticeshipQAApprenticeshipAssessment, UserInfo, ApprenticeshipQAApprenticeshipAssessment>(
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
