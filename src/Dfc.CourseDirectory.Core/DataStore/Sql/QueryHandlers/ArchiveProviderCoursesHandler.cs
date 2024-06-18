using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class ArchiveProviderCoursesHandler : ISqlQueryHandler<ArchiveProviderCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, ArchiveProviderCourses query)
        {
            var sql = $@"EXEC [Pttcd].[ArchiveProviderCourses] @RetentionDate";

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
