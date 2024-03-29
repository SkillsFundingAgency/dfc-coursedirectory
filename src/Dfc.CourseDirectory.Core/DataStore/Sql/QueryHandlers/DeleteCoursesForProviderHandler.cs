﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteCoursesForProviderHandler : ISqlQueryHandler<DeleteCoursesForProvider, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, DeleteCoursesForProvider query)
        {
            var sql = $@"
UPDATE Pttcd.CourseRuns
SET CourseRunStatus = {(int)CourseStatus.Archived}, UpdatedOn=GETDATE(),UpdatedBy='DeleteCoursesForProvider'
FROM Pttcd.CourseRuns cr
JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
WHERE cr.CourseRunStatus <> {(int)CourseStatus.Archived}
AND c.ProviderId = @ProviderId

UPDATE Pttcd.Courses
SET CourseStatus = {(int)CourseStatus.Archived}, UpdatedOn=GETDATE(),UpdatedBy='DeleteCoursesForProvider'
WHERE CourseStatus <> {(int)CourseStatus.Archived}
AND ProviderId = @ProviderId

UPDATE Pttcd.FindACourseIndex
SET Live = 0, UpdatedOn=GETDATE()
WHERE ProviderId = @ProviderId
AND OfferingType=1";

            var paramz = new
            {
                query.ProviderId
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
