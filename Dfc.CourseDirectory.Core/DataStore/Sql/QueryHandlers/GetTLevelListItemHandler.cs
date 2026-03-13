using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelListItemHandler : ISqlQueryHandler<GetTLevelsList, ListOfTLevels>
    {
        public async Task<ListOfTLevels> Execute(SqlTransaction transaction, GetTLevelsList query)
        {
            var sql = $@"SELECT count(*) as TLevelsCount
                            FROM Pttcd.TLevels t
                            JOIN Pttcd.Providers p ON t.ProviderId = p.ProviderId
                            Join Pttcd.ProviderContacts pc on p.[ProviderId] = pc.[ProviderId]
                            Join Pttcd.TLevelLocations tl on t.TLevelId = tl.TLevelId
                            where t.TLevelStatus = 1 and pc.ContactType = 'P';

                        SELECT
                            t.TLevelId,
                            d.Name as CourseName,
                            t.StartDate, 
                            t.Website as CourseWebsite,
                            t.WhoFor as WhoTheCourseIsFor,
                            t.EntryRequirements as EntryRequirements, 
                            t.WhatYoullLearn as WhatYoullLearn, 
                            t.HowYoullLearn,
                            t.HowYoullBeAssessed,
                            t.WhatYouCanDoNext, 
                            p.ProviderName,
                            pc.WebsiteAddress as ProviderWebsite,
                            pc.Email as ProviderEmail,
                            pc.Telephone1 AS ProviderPhoneNumber,
                            v.VenueName,
                            v.Postcode, 
                            v.AddressLine1, 
                            v.AddressLine2, 
                            v.Town,
                            v.County, 
                            v.Position.Lat Latitude, 
                            v.Position.Long Longitude,
                            d.QualificationLevel as TlevelQualificationLevel
                        FROM Pttcd.TLevels t
                        JOIN Pttcd.TLevelDefinitions d ON t.TLevelDefinitionId = d.TLevelDefinitionId
                        JOIN Pttcd.Providers p ON t.ProviderId = p.ProviderId
                        Join Pttcd.ProviderContacts pc on p.[ProviderId] = pc.[ProviderId]
                        Join Pttcd.TLevelLocations tl on t.TLevelId = tl.TLevelId
                        Join Pttcd.Venues v on tl.VenueId = v.VenueId
                        WHERE t.TLevelStatus = 1 and pc.ContactType = 'P'
                        order by TLevelId
                        OFFSET (@PageNumber-1)*@PageSize ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

            var paramz = new
            {
                query.PageNumber,
                query.PageSize
            };

            var queryReader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var tLevelCount = (await queryReader.ReadFirstOrDefaultAsync<int>());
            var tLevels = (await queryReader.ReadAsync<TLevelListItem>()).ToList();
            
            var listOfCourses = new ListOfTLevels()
            {
                TLevelsCount = tLevelCount,
                TLevels = tLevels
            };

            return listOfCourses;
        }
    }
}
