using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteArchivedCoursesHandler : ISqlQueryHandler<DeleteArchivedCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, DeleteArchivedCourses query)
        {
            var sql = $@"EXEC [Pttcd].[RemoveRedundantRecords] @RetentionDate";

            await transaction.Connection.ExecuteAsync(
                sql,
                new
                {
                    query.RetentionDate
                },
                commandTimeout: 1200,
                transaction: transaction);
            
                return new Success();
        }
    }
}
