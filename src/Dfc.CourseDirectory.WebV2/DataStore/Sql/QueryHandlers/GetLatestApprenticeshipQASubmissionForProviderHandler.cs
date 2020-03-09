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
    public class GetLatestApprenticeshipQASubmissionForProviderHandler
        : ISqlQueryHandler<GetLatestApprenticeshipQASubmissionForProvider, OneOf<None, ApprenticeshipQASubmission>>
    {
        public async Task<OneOf<None, ApprenticeshipQASubmission>> Execute(
            SqlTransaction transaction,
            GetLatestApprenticeshipQASubmissionForProvider query)
        {
            var sql = @"
SELECT TOP 1
    s.ApprenticeshipQASubmissionId,
    s.ProviderId,
    s.SubmittedOn,
    s.ProviderMarketingInformation,
    s.Passed,
    s.ProviderAssessmentPassed,
    s.ApprenticeshipAssessmentsPassed,
    b.UserId,
    b.Email,
    b.FirstName,
    b.LastName,
    a.UserId,
    a.Email,
    a.FirstName,
    a.LastName
FROM Pttcd.ApprenticeshipQASubmissions s WITH (HOLDLOCK)
JOIN Pttcd.Users b ON s.SubmittedByUserId = b.UserId
LEFT JOIN Pttcd.Users a ON s.LastAssessedByUserId = a.UserId
WHERE s.ProviderId = @ProviderId
ORDER BY s.SubmittedOn DESC";

            var paramz = new { query.ProviderId };

            var result = (await transaction.Connection.QueryAsync<ApprenticeshipQASubmission, UserInfo, UserInfo, ApprenticeshipQASubmission>(
                sql,
                (r, submittedBy, assessedBy) =>
                {
                    r.SubmittedByUser = submittedBy;
                    r.LastAssessedBy = assessedBy;
                    return r;
                },
                paramz,
                transaction,
                splitOn: "UserId,UserId")).SingleOrDefault();

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
