﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseUploadInvalidRowCountHandler : ISqlQueryHandler<GetCourseUploadInvalidRowCount, int>
    {
        public Task<int> Execute(SqlTransaction transaction, GetCourseUploadInvalidRowCount query)
        {
            var sql = $@"
SELECT COUNT(1) FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
AND IsValid = 0";

            return transaction.Connection.QuerySingleAsync<int>(sql, new { query.CourseUploadId }, transaction);
        }
    }
}
