using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateApprenticeshipQASubmissionHandler
        : ISqlQueryHandler<UpdateApprenticeshipQASubmission, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            SqlTransaction transaction,
            UpdateApprenticeshipQASubmission query)
        {
            var sql = @"
UPDATE Pttcd.ApprenticeshipQASubmissions SET
    Passed = @Passed,
    LastAssessedByUserId = @LastAssessedByUserId,
    LastAssessedOn = @LastAssessedOn,
    ProviderAssessmentPassed = @ProviderAssessmentPassed,
    ApprenticeshipAssessmentsPassed = @ApprenticeshipAssessmentsPassed
WHERE ApprenticeshipQASubmissionId = @ApprenticeshipQASubmissionId";

            var updated = await transaction.Connection.ExecuteAsync(sql, query, transaction);

            if (updated == 0)
            {
                return new NotFound();
            }
            else
            {
                return new Success();
            }
        }
    }
}
