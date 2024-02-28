using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateProviderNonLarsSubTypeHandler : ISqlQueryHandler<CreateProviderNonLarsSubType, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateProviderNonLarsSubType query)
        {
            const string sql = @"
                    INSERT INTO [Pttcd].[ProviderNonLarsSubType]
                               ([NonLarsSubTypeId]
                               ,[ProviderId])
                         VALUES (
                                @NonLarsSubTypeId,
                                @ProviderId
                            )";
            await transaction.Connection.ExecuteAsync(sql, query, transaction);

            return new Success();
        }
    }
}
