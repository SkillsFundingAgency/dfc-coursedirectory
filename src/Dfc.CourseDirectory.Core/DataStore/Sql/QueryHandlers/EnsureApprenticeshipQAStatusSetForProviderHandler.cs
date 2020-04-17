using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class EnsureApprenticeshipQAStatusSetForProviderHandler
        : ISqlQueryHandler<EnsureApprenticeshipQAStatusSetForProvider, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, EnsureApprenticeshipQAStatusSetForProvider query)
        {
            var sql = @"
MERGE Pttcd.Providers AS target
USING (SELECT @ProviderId ProviderId) AS source
ON source.ProviderId = target.ProviderId
WHEN NOT MATCHED THEN INSERT (ProviderId, ApprenticeshipQAStatus) VALUES (source.ProviderId, 0);";

            var paramz = new { query.ProviderId };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new None();
        }
    }
}
