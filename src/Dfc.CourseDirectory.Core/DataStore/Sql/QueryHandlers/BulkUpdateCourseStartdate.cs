using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;


namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class BulkUpdateCourseStartdate : ISqlQueryHandler<CourseStarteDateBulkUpdate, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CourseStarteDateBulkUpdate query)
        {
            var sql = $@"
    UPDATE Pttcd.CourseRuns 
    
    SET StartDate = @StartDate
    
    WHERE CourseId IN @selectedCourseRunResults";

            Guid[] selectedCourseRunResults = query.SelectedCourseRunid; 

            var paramz = new
            {
                selectedCourseRunResults,
    
                query.StartDate
         
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();

        }
    }
}
