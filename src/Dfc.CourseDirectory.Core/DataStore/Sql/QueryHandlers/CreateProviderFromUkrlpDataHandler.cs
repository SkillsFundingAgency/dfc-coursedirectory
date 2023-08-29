using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateProviderFromUkrlpDataHandler : ISqlQueryHandler<CreateProviderFromUkrlpData, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateProviderFromUkrlpDataHandler query)
        {
            var sql = $@"
INSERT INTO 
";

            var paramz = new
            {
                query.ProviderId,
                query.Ukprn,
                query.ProviderName,
                query.Alias,
                query.ProviderStatus,
                query.ProviderType,
                query.Status,
                query.DateUpdated,
                query.UpdatedBy
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
