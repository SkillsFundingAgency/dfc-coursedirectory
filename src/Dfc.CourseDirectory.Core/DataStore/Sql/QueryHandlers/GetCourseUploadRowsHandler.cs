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
        ISqlQueryHandler<GetCourseUploadRows, IReadOnlyCollection<CourseUploadRow>>
    {
        public async Task<IReadOnlyCollection<CourseUploadRow>> Execute(
            SqlTransaction transaction,
            GetCourseUploadRows query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, CourseId, CourseRunId, LastUpdated, LastValidated,
    LarsQan, WhoThisCourseIsFor, EntryRequirements, WhatYouWillLearn, HowYouWillLearn, WhatYouWillNeedToBring,
    HowYouWillBeAssessed, WhereNext, CourseName, YourReference, DeliveryMode, StartDate, FlexibleStartDate,
    VenueName, ProviderVenueRef, NationalDelivery, SubRegions, CourseWebpage, Cost, CostDescription,
    Duration, DurationUnit, StudyMode, AttendancePattern
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var results = (await transaction.Connection.QueryAsync<Result>(sql, new { query.CourseUploadId }, transaction))
                .AsList();

            foreach (var row in results)
            {
                row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return results;
        }

        private class Result : CourseUploadRow
        {
            public string ErrorList { get; set; }
            public UploadRowStatus CourseUploadRowStatus { get; set; }
        }
    }
}
