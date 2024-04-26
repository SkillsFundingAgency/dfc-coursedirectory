using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteArchivedCoursesHandler : ISqlQueryHandler<DeleteArchivedCourses, int>
    {
        public Task<int> Execute(SqlTransaction transaction, DeleteArchivedCourses query)
        {
            var sql = $@"EXEC Pttcd.DeleteArchivedCourses @RetentionDate";

            return transaction.Connection.QuerySingleAsync<int>(
                sql,
                new
                {
                    query.RetentionDate
                },
                commandTimeout: 180,
                transaction: transaction);
        }
    }
}
