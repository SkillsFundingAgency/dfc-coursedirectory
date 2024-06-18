using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class ArchiveOldCoursesHandler : ISqlQueryHandler<ArchiveOldCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, ArchiveOldCourses query)
        {
            var sql = $@"EXEC [Pttcd].[ArchiveOldCourses] @RetentionDate";

            await transaction.Connection.ExecuteAsync(
                sql,
                new
                {
                    query.RetentionDate
                },
                commandTimeout: 300,
                transaction: transaction);
            
                return new Success();
        }
    }
}
