using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipQASubmissionHandler : ISqlQueryHandler<CreateApprenticeshipQASubmission, int>
    {
        public Task<int> Execute(SqlTransaction transaction, CreateApprenticeshipQASubmission query)
        {
            var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissions
(ProviderId, SubmittedOn, SubmittedByUserId, ProviderBriefOverview)
VALUES (@ProviderId, @SubmittedOn, @SubmittedByUserId, @ProviderBriefOverview)

SELECT SCOPE_IDENTITY() ApprenticeshipQASubmissionId";

            var paramz = new
            {
                query.ProviderId,
                query.SubmittedOn,
                query.SubmittedByUserId,
                query.ProviderBriefOverview
            };

            return transaction.Connection.QuerySingleAsync<int>(sql, paramz, transaction);
        }
    }
}
