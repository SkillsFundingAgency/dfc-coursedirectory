using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetDeadlockPriorityHandler : ISqlQueryHandler<SetDeadlockPriority, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, SetDeadlockPriority query)
        {
            var sql = @"SET DEADLOCK_PRIORITY @Priority";

            var paramz = new { Priority = query.Priority };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new None();
        }
    }
}
