using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetExpiredCourseRunsForProviderHandler :
        ISqlQueryHandler<GetExpiredCourseRunsForProvider, IReadOnlyCollection<ExpiredCourseRunResult>>
    {
        public async Task<IReadOnlyCollection<ExpiredCourseRunResult>> Execute(
            SqlTransaction transaction,
            GetExpiredCourseRunsForProvider query)
        {
            var sql = $@"SELECT c.CourseId, cr.CourseRunId, cr.CourseName, cr.ProviderCourseId, c.LearnAimRef, cr.DeliveryMode,
                v.VenueName, cr.[National], cr.StudyMode, ld.LearnAimRefTitle, ld.NotionalNVQLevelv2, ld.AwardOrgCode, lart.LearnAimRefTypeDesc,
                cr.StartDate, 'Lars' AS Type
            FROM Pttcd.Courses c
            JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
            JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
            JOIN LARS.LearningDelivery ld ON c.LearnAimRef = ld.LearnAimRef
            JOIN LARS.LearnAimRefType lart ON ld.LearnAimRefType = lart.LearnAimRefType
            LEFT JOIN Pttcd.Venues v ON cr.VenueId = v.VenueId
            WHERE cr.CourseRunStatus = 1
            AND cr.StartDate < @Today
            AND p.ProviderId = @ProviderId
            UNION
            SELECT top 2 c.CourseId, cr.CourseRunId, cr.CourseName, cr.ProviderCourseId, c.LearnAimRef, cr.DeliveryMode,
	            v.VenueName, cr.[National], cr.StudyMode, '' AS LearnAimRefTitle, '' AS NotionalNVQLevelv2, '' AS AwardOrgCode, '' AS LearnAimRefTypeDesc, 
	            cr.StartDate, 'NonLars' AS Type
            FROM Pttcd.Courses c
            JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId and c.LearnAimRef is null
            JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
            LEFT JOIN Pttcd.Venues v ON cr.VenueId = v.VenueId
            WHERE cr.CourseRunStatus = 1
            AND cr.StartDate < @Today
            AND p.ProviderId = @ProviderId
            UNION
            SELECT
                t.TLevelId AS CourseId, t.TLevelId AS 'CourseRunId', d.Name AS CourseName, T.YourReference AS 'ProviderCourseId', '' AS 'LearnAimRef', 0 AS 'DeliveryMode', 
	            v.VenueName, 1 AS 'National', 1 AS 'StudyMode', '' AS 'LearnAimRefTitle', '' AS 'NotionalNVQLevelv2', '' AS 'AwardOrgCode', '' AS 'LearnAimRefTypeDesc' , 
	            t.StartDate, 'TLevel' AS Type
            FROM Pttcd.TLevels AS T
            JOIN Pttcd.TLevelDefinitions d ON t.TLevelDefinitionId = d.TLevelDefinitionId
            JOIN Pttcd.Providers p ON t.ProviderId = p.ProviderId
            JOIN Pttcd.TLevelLocations AS tll ON tll.TLevelId = t.TLevelId
            JOIN Pttcd.Venues v ON tll.VenueId = v.VenueId
            WHERE t.ProviderId = @ProviderId
	            AND t.TLevelStatus = 1
	            AND t.StartDate < @Today
            ORDER BY Type";

            sql += $@"
                    SELECT crsr.CourseRunId, crsr.RegionId
                    FROM Pttcd.Courses c
                        JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
                        JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
                        JOIN Pttcd.CourseRunSubRegions crsr ON cr.CourseRunId = crsr.CourseRunId
                    WHERE cr.CourseRunStatus = 1 AND cr.StartDate < @Today AND p.ProviderId = @ProviderId";

            var parameters = new
            {
                query.ProviderId,
                query.Today
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, parameters, transaction);

            var results = (await reader.ReadAsync<ExpiredCourseRunResult>()).AsList();

            var subRegions = (await reader.ReadAsync<SubRegionResult>()).ToLookup(r => r.CourseRunId, r => r.RegionId);

            foreach (var result in results)
            {
                result.SubRegionIds = (subRegions.Contains(result.CourseRunId) ?
                    subRegions[result.CourseRunId] :
                    Enumerable.Empty<string>()).ToArray();
            }

            return results;
        }

        private class SubRegionResult
        {
            public Guid CourseRunId { get; set; }
            public string RegionId { get; set; }
        }
    }
}
