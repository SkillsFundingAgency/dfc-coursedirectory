using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateNonLarsSubTypeHandler : ISqlQueryHandler<CreateNonLarsSubType, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateNonLarsSubType query)
        {
            const string sql = @"
                    INSERT INTO [Pttcd].[NonLarsSubType]
                               ([NonLarsSubTypeId]
                               ,[Name]
                               ,[AddedOn]
                               ,[IsActive])
                         VALUES (
                                @NonLarsSubTypeId,
                                @Name,
                                @AddedOn,
                                @IsActive
                            )";
            await transaction.Connection.ExecuteAsync(sql, query, transaction);

            return new Success();
        }
    }
}
