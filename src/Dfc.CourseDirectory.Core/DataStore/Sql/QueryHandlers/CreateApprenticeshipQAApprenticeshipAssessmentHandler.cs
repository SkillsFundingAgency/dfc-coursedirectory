using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipQAApprenticeshipAssessmentHandler
        : ISqlQueryHandler<CreateApprenticeshipQAApprenticeshipAssessment, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateApprenticeshipQAApprenticeshipAssessment query)
        {
            var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissionApprenticeshipAssessments (
    ApprenticeshipQASubmissionApprenticeshipId,
    AssessedOn,
    AssessedByUserId,
    Passed,
    CompliancePassed,
    ComplianceFailedReasons,
    ComplianceComments,
    StylePassed,
    StyleFailedReasons,
    StyleComments)
SELECT
    ApprenticeshipQASubmissionApprenticeshipId,
    @AssessedOn,
    @AssessedByUserId,
    @Passed,
    @CompliancePassed,
    @ComplianceFailedReasons,
    @ComplianceComments,
    @StylePassed,
    @StyleFailedReasons,
    @StyleComments
FROM Pttcd.ApprenticeshipQASubmissionApprenticeships
WHERE ApprenticeshipId = @ApprenticeshipId
AND ApprenticeshipQASubmissionId = @ApprenticeshipQASubmissionId";

            await transaction.Connection.ExecuteAsync(sql, query, transaction);

            return new Success();
        }
    }
}
