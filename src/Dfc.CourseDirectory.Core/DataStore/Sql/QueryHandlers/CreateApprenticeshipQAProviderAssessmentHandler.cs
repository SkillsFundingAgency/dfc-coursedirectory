using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipQAProviderAssessmentHandler
        : ISqlQueryHandler<CreateApprenticeshipQAProviderAssessment, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateApprenticeshipQAProviderAssessment query)
        {
            var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissionProviderAssessments (
    ApprenticeshipQASubmissionId,
    AssessedOn,
    AssessedByUserId,
    Passed,
    CompliancePassed,
    ComplianceFailedReasons,
    ComplianceComments,
    StylePassed,
    StyleFailedReasons,
    StyleComments)
VALUES (
    @ApprenticeshipQASubmissionId,
    @AssessedOn,
    @AssessedByUserId,
    @Passed,
    @CompliancePassed,
    @ComplianceFailedReasons,
    @ComplianceComments,
    @StylePassed,
    @StyleFailedReasons,
    @StyleComments)";

            await transaction.Connection.ExecuteAsync(sql, query, transaction);

            return new Success();
        }
    }
}
