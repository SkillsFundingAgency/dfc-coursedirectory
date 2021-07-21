using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseUploadRowDetailHandler : ISqlQueryHandler<GetCourseUploadRowDetail, CourseUploadRowDetail>
    {
        public async Task<CourseUploadRowDetail> Execute(
            SqlTransaction transaction,
            GetCourseUploadRowDetail query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, CourseId, CourseRunId, LastUpdated, LastValidated,
    LearnAimRef, WhoThisCourseIsFor, EntryRequirements, WhatYouWillLearn, HowYouWillLearn, WhatYouWillNeedToBring,
    HowYouWillBeAssessed, WhereNext, CourseName, ProviderCourseRef, DeliveryMode, StartDate, FlexibleStartDate,
    VenueName, ProviderVenueRef, NationalDelivery, SubRegions, CourseWebpage, Cost, CostDescription,
    Duration, DurationUnit, StudyMode, AttendancePattern, VenueId,
    ResolvedDeliveryMode, ResolvedStartDate, ResolvedFlexibleStartDate, ResolvedNationalDelivery,
    ResolvedCost, ResolvedDuration, ResolvedDurationUnit, ResolvedStudyMode, ResolvedAttendancePattern
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId AND RowNumber = @RowNumber
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber

SELECT RegionId FROM Pttcd.CourseUploadRowSubRegions
WHERE CourseUploadId = @CourseUploadId AND RowNumber = @RowNumber
";

            var paramz = new
            {
                query.CourseUploadId,
                query.RowNumber
            };

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction))
            {
                var result = await reader.ReadSingleOrDefaultAsync<Result>();

                if (result == null)
                {
                    return null;
                }

                result.Errors = (result.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);

                result.ResolvedSubRegionIds = (await reader.ReadAsync<string>()).AsList();

                return result;
            }
        }

        private class Result : CourseUploadRowDetail
        {
            public string ErrorList { get; set; }
        }
    }
}
