using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetExclusiveLockHandler : ISqlQueryHandler<GetExclusiveLock, bool>
    {
        public async Task<bool> Execute(SqlTransaction transaction, GetExclusiveLock query)
        {
            var param = new DynamicParameters(new
            {
                Resource = query.Name,
                LockMode = "Exclusive",
                LockTimeout = query.TimeoutMilliseconds
            });

            param.Add(name: "@RetVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            await transaction.Connection.ExecuteAsync(
                "sp_getapplock",
                param,
                transaction,
                commandTimeout: 0,
                commandType: CommandType.StoredProcedure);

            var result = param.Get<int>("@RetVal");

            // See https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver15#return-code-values
            return result switch
            {
                0 => true,
                1 => true,
                _ => false
            };
        }
    }
}
