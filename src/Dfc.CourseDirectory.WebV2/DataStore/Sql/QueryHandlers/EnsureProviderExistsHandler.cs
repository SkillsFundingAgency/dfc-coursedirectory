using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class EnsureProviderExistsHandler : ISqlQueryHandler<EnsureProviderExists, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, EnsureProviderExists query)
        {
            var sql = @"
MERGE Pttcd.Providers AS target
USING (SELECT @ProviderId ProviderId) AS source
ON target.ProviderId = source.ProviderId
WHEN NOT MATCHED THEN INSERT (ProviderId) VALUES (source.ProviderId);";

            var paramz = new { query.ProviderId };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new None();
        }
    }
}
