using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseListItemHandler : ISqlQueryHandler<GetCourseList, ListOfCourses>
    {
        public async Task<ListOfCourses> Execute(SqlTransaction transaction, GetCourseList query)
        {
            var sql = $@"SELECT 
                            count(cr.CourseRunId) AS CourseCount
                        FROM Pttcd.CourseRuns cr
                        LEFT JOIN Pttcd.Courses c on cr.CourseId = c.CourseId
                        LEFT JOIN Pttcd.Providers p on c.ProviderId = p.ProviderId
                        LEFT JOIN Pttcd.ProviderContacts pc on p.ProviderId = pc.ProviderId
                        LEFT JOIN PTTCD.CourseRunSubRegions crsr on cr.CourseRunId = crsr.CourseRunId
                        LEFT JOIN pttcd.Regions sr on crsr.RegionId = sr.RegionId
                        WHERE cr.CourseRunStatus = 1 and pc.ContactType = 'P';

                        SELECT
                            cr.CourseRunId AS Id,
                            cr.CourseId,
                            cr.CourseName,
                            c.CourseType,
                            s.[Description] AS SectorDescription,
                            s.Code As SectorCode,
                            SSA.SectorSubjectAreaTier2Desc AS SectorSubjectArea,
                            c.EducationLevel,
                            c.AwardingBody,
                            cr.DeliveryMode,
                            cr.FlexibleStartDate,
                            cr.StartDate,
                            cr.CourseWebsite,
                            cr.Cost,
                            cr.CostDescription,
                            cr.DurationUnit,
                            cr.DurationValue,
                            cr.StudyMode,
                            cr.AttendancePattern, 
                            cr.[National],
                            sr.Name AS Region,
                            r.Name AS ParentRegion,
                            c.CourseDescription AS WhoTheCourseIsFor,
                            c.EntryRequirements,
                            c.WhatYoullLearn,
                            c.HowYoullLearn,
                            c.WhatYoullNeed,
                            c.HowYoullBeAssessed,
                            c.WhereNext AS WhatYouCanDoNext,
                            p.ProviderName,
                            pc.WebsiteAddress AS ProviderWebsite,
                            pc.Email AS ProviderEmail,
                            pc.Telephone1 AS ProviderPhoneNumber,
                            v.VenueName,
                            v.Postcode, 
                            v.AddressLine1, 
                            v.AddressLine2, 
                            v.Town,
                            v.County, 
                            v.Position.Lat Latitude, 
                            v.Position.Long Longitude,  
                            l.LearnAimRef,
                            l.LearnAimRefTitle,
                            l.NotionalNVQLevelv2 AS QualificationLevel,
                            l.AwardOrgCode AS AwardingOrganisation
                        FROM Pttcd.CourseRuns cr
                        LEFT JOIN Pttcd.Courses c on cr.CourseId = c.CourseId
                        LEFT JOIN Pttcd.Venues v on v.VenueId = cr.VenueId
                        LEFT JOIN Pttcd.Sectors s on c.SectorId = s.Id
                        LEFT JOIN Pttcd.Providers p on c.ProviderId = p.ProviderId
                        LEFT JOIN Pttcd.ProviderContacts pc on p.ProviderId = pc.ProviderId
                        LEFT JOIN PTTCD.CourseRunSubRegions crsr on cr.CourseRunId = crsr.CourseRunId
                        LEFT JOIN pttcd.Regions sr on crsr.RegionId = sr.RegionId
                        LEFT JOIN pttcd.regions r on sr.ParentRegionId = r.RegionId
                        LEFT JOIN lars.LearningDelivery l on c.LearnAimRef = l.LearnAimRef
                        LEFT JOIN lars.SectorSubjectAreaTier2 SSA on l.[SectorSubjectAreaTier2] = SSA.SectorSubjectAreaTier2
                        WHERE cr.CourseRunStatus = 1 and pc.ContactType = 'P'
                        order by Id
                        OFFSET (@PageNumber-1)*@PageSize ROWS
                        FETCH NEXT @PageSize ROWS ONLY
                ";

            var paramz = new
            {
                query.PageNumber,
                query.PageSize
            };

            using var queryReader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);
            var courseCount = (await queryReader.ReadFirstOrDefaultAsync<int>());
            var courses = (await queryReader.ReadAsync<CourseListItem>()).ToList();

            var listOfCourses = new ListOfCourses()
            {
                CourseCount = courseCount,
                Courses = courses
            };
            return listOfCourses;
        }
    }
}
