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
    public class GetCourseUploadRowsToRevalidateHandler :
        ISqlQueryHandler<GetCourseUploadRowsToRevalidate, IReadOnlyCollection<CourseUploadRow>>
    {
        public async Task<IReadOnlyCollection<CourseUploadRow>> Execute(SqlTransaction transaction, GetCourseUploadRowsToRevalidate query)
        {
            var sql = $@"
DECLARE @LastVenueChangeForProvider DATETIME

SELECT @LastVenueChangeForProvider = MAX(ISNULL(v.UpdatedOn, v.CreatedOn))
FROM Pttcd.CourseUploads cu
JOIN Pttcd.Providers p ON cu.ProviderId = p.ProviderId
JOIN Pttcd.Venues v ON p.Ukprn = v.ProviderUkprn
WHERE cu.CourseUploadId = @CourseUploadId

SELECT
    cur.RowNumber, cur.IsValid, cur.Errors AS ErrorList, cur.CourseId, cur.CourseRunId, cur.LastUpdated, cur.LastValidated,
    cur.LearnAimRef, cur.WhoThisCourseIsFor, cur.EntryRequirements, cur.WhatYouWillLearn, cur.HowYouWillLearn, cur.WhatYouWillNeedToBring,
    cur.HowYouWillBeAssessed, cur.WhereNext, cur.CourseName, cur.ProviderCourseRef, cur.DeliveryMode, cur.StartDate, cur.FlexibleStartDate,
    cur.VenueName, cur.ProviderVenueRef, cur.NationalDelivery, cur.SubRegions, cur.CourseWebpage, cur.Cost, cur.CostDescription,
    cur.Duration, cur.DurationUnit, cur.StudyMode, cur.AttendancePattern, cur.VenueId
FROM Pttcd.CourseUploadRows cur
LEFT JOIN Pttcd.Venues v ON cur.VenueId = v.VenueId
WHERE cur.CourseUploadId = @CourseUploadId
AND cur.CourseUploadRowStatus = {(int)UploadRowStatus.Default}
AND cur.ResolvedDeliveryMode = {(int)CourseDeliveryMode.ClassroomBased}
AND (
    -- Matched venue has been updated or deleted since we last validated row
    (cur.VenueId IS NOT NULL AND ISNULL(v.UpdatedOn, v.CreatedOn) > cur.LastValidated)

    OR

    -- Don't have a matched venue yet but a venue for the provider has been changed since last validated row
    (cur.VenueId IS NULL AND @LastVenueChangeForProvider IS NOT NULL AND @LastVenueChangeForProvider > cur.LastValidated)
)
ORDER BY cur.RowNumber";

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
