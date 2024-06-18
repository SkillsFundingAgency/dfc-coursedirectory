using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers.OpenData
{
    public class GetLiveCourseRunsWithRegionsAndVenuesReportHandler
        : ISqlAsyncEnumerableQueryHandler<GetLiveCoursesWithRegionsAndVenuesReport, LiveCoursesWithRegionsAndVenuesReportItem>
    {
        public async IAsyncEnumerable<LiveCoursesWithRegionsAndVenuesReportItem> Execute(SqlTransaction transaction, GetLiveCoursesWithRegionsAndVenuesReport query)
        {            
            var sql = @$"
WITH cte_CourseRunIds AS (
    SELECT DISTINCT c.CourseRunId 
    FROM [Pttcd].[FindACourseIndex] c
    WHERE c.OfferingType = 1
        AND c.Live = 1
        AND (c.FlexibleStartDate = 1 OR c.StartDate >= '{query.FromDate:MM-dd-yyyy}')
),
cte_course_run_regions AS (
    SELECT 
        CourseRunId, 
        STRING_AGG(c.RegionName, ',') WITHIN GROUP (ORDER BY c.CourseRunId, c.RegionName) AS Regions
    FROM [Pttcd].[FindACourseIndex] c
    GROUP BY CourseRunId
)
SELECT
    c.ProviderUkprn,
    p.ProviderName,
    cr.CourseRunId,
    c.CourseId,
    c.LearnAimRef,
    cr.CourseName,
    c.CourseDescription, 
    cr.CourseWebsite,
    CONVERT(DECIMAL(10,2), cr.Cost) AS Cost,
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
    r.Regions,
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
    cr.CreatedOn,
    v.Website AS VenueWebsite,
    cr.UpdatedOn,    
	c.CourseType,
    c.SectorId,
    c.EducationLevel, 
    c.AwardingBody
FROM [Pttcd].[CourseRuns] cr
INNER JOIN [Pttcd].[Courses] c ON c.CourseId = cr.CourseId
INNER JOIN [Pttcd].[Providers] p ON p.ProviderId = c.ProviderId 
LEFT JOIN [Pttcd].[Venues] v ON cr.VenueId = v.VenueId
LEFT JOIN cte_course_run_regions r ON cr.CourseRunId = r.CourseRunId
WHERE p.ProviderType != ({(int)ProviderType.None})
    AND cr.CourseRunId IN (SELECT CourseRunId FROM cte_CourseRunIds)
UNION 
SELECT
    p.Ukprn AS ProviderUkprn,
    p.ProviderName,
    tl.TLevelLocationId AS CourseRunId,
    t.TLevelId AS CourseId,
    NULL AS LearnAimRef,
    tld.[Name] AS CourseName,
    t.WhatYoullLearn AS CourseDescription, 
    t.Website AS CourseWebsite,
    NULL AS Cost,
    'T Levels are currently only available to 16-19 year olds. Contact us for details of other suitable courses.' CostDescription,
    CAST(0 AS bit) FlexibleStartDate,
    t.StartDate,        
    t.EntryRequirements,
    t.HowYoullBeAssessed,
    1 DeliveryMode,  -- Classroom based
    1 AttendancePattern, -- Day time
	1 StudyMode,  -- Full time
    4 DurationUnit,  -- Years
    2 DurationValue,                
    NULL [National],
    NULL Regions,
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
    t.CreatedOn,
    v.Website AS VenueWebsite,
    t.UpdatedOn,    
	2 AS CourseType,
    NULL SectorId,
    NULL EducationLevel, 
    NULL AwardingBody
FROM [Pttcd].[TLevels] t
INNER JOIN [Pttcd].[TLevelLocations] tl ON t.TLevelId = tl.TLevelId
INNER JOIN Pttcd.TLevelDefinitions tld ON t.TLevelDefinitionId = tld.TLevelDefinitionId
INNER JOIN [Pttcd].[Providers] p ON p.ProviderId = t.ProviderId
INNER JOIN [Pttcd].[Venues] v ON tl.VenueId = v.VenueId
WHERE t.TLevelStatus = 1 
AND tl.TLevelLocationStatus = 1 
AND p.ProviderType IN ({(int)ProviderType.TLevels}, {(int)ProviderType.FE + (int)ProviderType.TLevels}, {(int)ProviderType.TLevels + (int)ProviderType.NonLARS}, {(int)ProviderType.FE + (int)ProviderType.NonLARS + (int)ProviderType.TLevels})
AND t.StartDate >= '{query.FromDate:MM-dd-yyyy}'
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
