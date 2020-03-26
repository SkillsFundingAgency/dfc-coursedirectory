using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
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

            return await transaction.Connection.QuerySingleOrDefaultAsync<ApprenticeshipQAStatus?>(sql, paramz, transaction);
        }
    }
}
