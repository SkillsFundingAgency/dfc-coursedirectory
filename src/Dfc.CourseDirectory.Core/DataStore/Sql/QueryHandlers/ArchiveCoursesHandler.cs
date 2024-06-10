using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class ArchiveCoursesHandler : ISqlQueryHandler<ArchiveCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, ArchiveCourses query)
        {
            var sql = $@"EXEC [Pttcd].[ArchiveCourses]";

            await transaction.Connection.ExecuteAsync(
                sql, 
                null,
                commandTimeout: 300,
                transaction: transaction);
            
                return new Success();
        }
    }
}
