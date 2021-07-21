using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseUploadRowsHandler :
        ISqlQueryHandler<GetCourseUploadRows, (IReadOnlyCollection<CourseUploadRow> ErrorRows, int TotalRows)>
    {
        public async Task<(IReadOnlyCollection<CourseUploadRow> ErrorRows, int TotalRows)> Execute(
            SqlTransaction transaction,
            GetCourseUploadRows query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, CourseId, CourseRunId, LastUpdated, LastValidated,
    LearnAimRef, WhoThisCourseIsFor, EntryRequirements, WhatYouWillLearn, HowYouWillLearn, WhatYouWillNeedToBring,
    HowYouWillBeAssessed, WhereNext, CourseName, ProviderCourseRef, DeliveryMode, StartDate, FlexibleStartDate,
    VenueName, ProviderVenueRef, NationalDelivery, SubRegions, CourseWebpage, Cost, CostDescription,
    Duration, DurationUnit, StudyMode, AttendancePattern, VenueId
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
{(query.WithErrorsOnly ? "AND IsValid = 0" : "")}
ORDER BY RowNumber";

            if (query.WithErrorsOnly)
            {
                sql += $@"
SELECT COUNT(*)
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}";
            }

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, new { query.CourseUploadId }, transaction))
            {
                var rows = (await reader.ReadAsync<Result>()).AsList();

                foreach (var row in rows)
                {
                    row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
                }

                var totalRows = query.WithErrorsOnly ? await reader.ReadSingleAsync<int>() : rows.Count;

                return (rows, totalRows);
            }
        }

        private class Result : CourseUploadRow
        {
            public string ErrorList { get; set; }
            public UploadRowStatus CourseUploadRowStatus { get; set; }
        }
    }
}
