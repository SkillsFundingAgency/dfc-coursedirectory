using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLiveCourseRunsWithRegionsAndVenuesReportHandler
        : ISqlAsyncEnumerableQueryHandler<GetLiveCourseProvidersReport, LiveCourseProvidersReportItem>
    {
        public async IAsyncEnumerable<LiveCourseProvidersReportItem> Execute(SqlTransaction transaction, GetLiveCourseProvidersReport query)
        {
            var sql = @$"
WITH 
cte_course_run_regions (courseRunId, regionList) AS (
    SELECT 
            CourseRunId, 
            STRING_AGG  (c.RegionName, ',') WITHIN GROUP (ORDER BY c.CourseRunId, c.RegionName) Regions
    FROM [Pttcd].[FindACourseIndex] c with (nolock)
    GROUP BY CourseRunId
)

SELECT
                c.ProviderUkprn,
                cr.CourseRunId,
                c.CourseId,
                c.LearnAimRef,
                cr.CourseName,
                c.CourseDescription,
                cr.CourseWebsite,
                CONVERT(DECIMAL(10,2),cr.Cost) AS Cost,
                cr.CostDescription,
                cr.FlexibleStartDate,
                cr.StartDate,        
                c.EntryRequirements,
                c.HowYoullBeAssessed AS HowYouWillBeAssessed,
                cr.DeliveryMode,
                cr.AttendancePattern,
                cr.StudyMode,
                cr.DurationUnit,
                cr.DurationValue,    
                ISNULL(cr.[National], 0) AS [National],
                r.regionList AS Regions,
                v.VenueName,
                v.AddressLine1 AS VenueAddress1,
                v.AddressLine2 AS VenueAddress2,
                v.County AS VenueCounty,
                v.Postcode AS VenuePostcode,
                v.Town AS VenueTown,
                v.Position.Lat AS VenueLatitude, 
                v.Position.Lon AS VenueLongitude,
                v.[Telephone] AS VenueTelephone,
                v.Email AS VenueEmail,
                v.Website AS VenueWebsite,
                cr.UpdatedOn
FROM            [Pttcd].[CourseRuns] cr
INNER JOIN      [Pttcd].[Courses] c ON c.CourseId = cr.CourseId
LEFT OUTER JOIN [Pttcd].[Venues] v with (nolock) ON cr.VenueId = v.VenueId
LEFT OUTER JOIN cte_course_run_regions r with (nolock) ON cr.CourseRunId = r.CourseRunId
WHERE           cr.CourseRunId IN (
                SELECT      DISTINCT c.CourseRunId FROM [Pttcd].[FindACourseIndex] c
                WHERE       c.OfferingType = {(int) FindACourseOfferingType.Course}
                AND         c.Live = 1
                AND         (c.FlexibleStartDate = 1 OR c.StartDate >= '{string.Format("dd-MM-yyyy", query.FromDate)}'));
)
ORDER BY        cr.StartDate, c.ProviderUkprn, c.LearnAimRef";

            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction))
            {
                var parser = reader.GetRowParser<LiveCourseProvidersReportItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
