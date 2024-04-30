using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteArchivedCoursesHandler : ISqlQueryHandler<DeleteArchivedCourses, int>
    {
        public Task<int> Execute(SqlTransaction transaction, DeleteArchivedCourses query)
        {
            var sql = $@"EXEC [Pttcd].[RemoveRedundantRecords] @RetentionDate";

            return transaction.Connection.QuerySingleAsync<int>(
                sql,
                new
                {
                    query.RetentionDate
                },
                commandTimeout: 1200,
                transaction: transaction);
        }
    }
}
