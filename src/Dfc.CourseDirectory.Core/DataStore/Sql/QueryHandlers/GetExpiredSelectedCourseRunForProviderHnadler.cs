using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
        public class GetExpiredSelectedCourseRunForProviderHnadler :
       ISqlQueryHandler<GetExpiredSelectedCourseRunsForProvider, IReadOnlyCollection<ExpiredCourseRunResult>>
        {
            public async Task<IReadOnlyCollection<ExpiredCourseRunResult>> Execute(
                SqlTransaction transaction,
                GetExpiredSelectedCourseRunsForProvider query)
            {
                var sql = $@"
SELECT
	c.CourseId, cr.CourseRunId, cr.CourseName, cr.ProviderCourseId, c.LearnAimRef, cr.DeliveryMode,
	v.VenueName, cr.[National], cr.StudyMode, ld.LearnAimRefTitle, ld.NotionalNVQLevelv2, ld.AwardOrgCode, lart.LearnAimRefTypeDesc,
	cr.StartDate
FROM Pttcd.Courses c
JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
JOIN LARS.LearningDelivery ld ON c.LearnAimRef = ld.LearnAimRef
JOIN LARS.LearnAimRefType lart ON ld.LearnAimRefType = lart.LearnAimRefType


LEFT JOIN Pttcd.Venues v ON cr.VenueId = v.VenueId
WHERE cr.CourseRunStatus = 1
AND cr.StartDate < @Today
AND p.ProviderId = @ProviderId

AND  cr.CourseId = @SelectedCourseRuns

ORDER BY ld.LearnAimRefTitle

SELECT
	crsr.CourseRunId, crsr.RegionId
FROM Pttcd.Courses c
JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
JOIN Pttcd.CourseRunSubRegions crsr ON cr.CourseRunId = crsr.CourseRunId
WHERE cr.CourseRunStatus = 1
AND cr.StartDate < @Today
AND p.ProviderId = @ProviderId
AND cr.CourseId = @SelectedCourseRuns";

            var paramz = new
            {
                    query.SelectedCourseRuns,
                    query.ProviderId,
                    query.Today
                };

                using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

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

