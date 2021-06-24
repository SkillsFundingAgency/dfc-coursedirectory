using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseUploadRowHandler : ISqlQueryHandler<GetCourseUploadRow, CourseUploadRow>
    {
        public async Task<CourseUploadRow> Execute(
            SqlTransaction transaction,
            GetCourseUploadRow query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, CourseId, CourseRunId, LastUpdated, LastValidated,
    LarsQan, WhoThisCourseIsFor, EntryRequirements, WhatYouWillLearn, HowYouWillLearn, WhatYouWillNeedToBring,
    HowYouWillBeAssessed, WhereNext, CourseName, ProviderCourseRef, DeliveryMode, StartDate, FlexibleStartDate,
    VenueName, ProviderVenueRef, NationalDelivery, SubRegions, CourseWebpage, Cost, CostDescription,
    Duration, DurationUnit, StudyMode, AttendancePattern, VenueId
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId AND RowNumber = @RowNumber
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var paramz = new
            {
                query.CourseUploadId,
                query.RowNumber
            };

            var result = await transaction.Connection.QuerySingleOrDefaultAsync<Result>(sql, paramz, transaction);

            if (result != null)
            {
                result.Errors = (result.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return result;
        }

        private class Result : CourseUploadRow
        {
            public string ErrorList { get; set; }
            public UploadRowStatus CourseUploadRowStatus { get; set; }
        }
    }
}
