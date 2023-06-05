using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers.OpenData
{
    public class GetLiveCourseRunsWithRegionsAndVenuesReportHandler
        : ISqlAsyncEnumerableQueryHandler<GetLiveCoursesWithRegionsAndVenuesReport, LiveCoursesWithRegionsAndVenuesReportItem>
    {
        public async IAsyncEnumerable<LiveCoursesWithRegionsAndVenuesReportItem> Execute(SqlTransaction transaction, GetLiveCoursesWithRegionsAndVenuesReport query)
        {
            //Note about CourseDescription column in below SQL query            
            //In Database, CourseDescription column of Course table maps with Add/Edit Course form field @WhoThisCourseIsFor
            //In Report, COURSE_DESCRIPTION column maps with 'c.WhatYoullLearn as CourseDescription' column of below SQL query which maps with Add/Edit Course form field @WhatYoullLearn. 
            //This is done as part of working on Jira ID NCSLT-1099 ticket
            var sql = @$"

CREATE TABLE #CourseRunIds (
    CourseRunId UNIQUEIDENTIFIER
)

INSERT INTO #CourseRunIds (CourseRunId)
SELECT DISTINCT c.CourseRunId 
FROM [Pttcd].[FindACourseIndex] c
WHERE c.OfferingType = {(int)FindACourseOfferingType.Course}
AND c.Live = 1
AND (c.FlexibleStartDate = 1 OR c.StartDate >= '{query.FromDate:MM-dd-yyyy}');


;WITH 
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
                c.WhatYoullLearn as CourseDescription, 
                cr.CourseWebsite,
                CONVERT(DECIMAL(10,2),cr.Cost) AS Cost,
                cr.CostDescription,
                cr.FlexibleStartDate,
                cr.StartDate,        
                c.EntryRequirements,
                c.HowYoullBeAssessed,
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
                v.Position.Long AS VenueLongitude,
                v.Telephone AS VenueTelephone,
                v.Email AS VenueEmail,
                v.Website AS VenueWebsite,
                cr.UpdatedOn
FROM            [Pttcd].[CourseRuns] cr
INNER JOIN      [Pttcd].[Courses] c ON c.CourseId = cr.CourseId
INNER JOIN		[Pttcd].[Providers] p ON p.ProviderId = c.ProviderId 
LEFT OUTER JOIN [Pttcd].[Venues] v with (nolock) ON cr.VenueId = v.VenueId
LEFT OUTER JOIN cte_course_run_regions r with (nolock) ON cr.CourseRunId = r.CourseRunId
WHERE           p.ProviderType IN ({(int)ProviderType.FE}, {(int)ProviderType.FE + (int)ProviderType.TLevels})
AND
                cr.CourseRunId IN (SELECT CourseRunId FROM #CourseRunIds)


DROP TABLE #CourseRunIds;

";

            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction, commandTimeout: 200))
            {
                var parser = reader.GetRowParser<LiveCoursesWithRegionsAndVenuesReportItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
