using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderApprenticeshipQAStatusHandler :
        ISqlQueryHandler<GetProviderApprenticeshipQAStatus, ApprenticeshipQAStatus?>
    {
        public async Task<ApprenticeshipQAStatus?> Execute(
            SqlTransaction transaction,
            GetProviderApprenticeshipQAStatus query)
        {
            var sql = @"
SELECT ApprenticeshipQAStatus FROM Pttcd.Providers WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId";

            var paramz = new { query.ProviderId };

#if DEBUG
            if (bool.TryParse(System.Environment.GetEnvironmentVariable("CD_BYPASS_QA"), out var bypassQA) && bypassQA)
            {
                return ApprenticeshipQAStatus.Passed;
            }
#endif
            return await transaction.Connection.QuerySingleOrDefaultAsync<ApprenticeshipQAStatus?>(sql, paramz, transaction);
        }
    }
}
