using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelUpdateItemHandler : ISqlQueryHandler<GetTLevelUpdates, ListOfTLevelUpdates>
    {
        public async Task<ListOfTLevelUpdates> Execute(SqlTransaction transaction, GetTLevelUpdates query)
        {
            var sql = $@"SELECT
                            count(*) as TLevelCount
                        FROM Pttcd.TLevels t
                        JOIN Pttcd.Providers p ON t.ProviderId = p.ProviderId
                        Join Pttcd.ProviderContacts pc on p.[ProviderId] = pc.[ProviderId]
                        Join Pttcd.TLevelLocations tl on t.TLevelId = tl.TLevelId
                        where t.TLevelStatus = 1 and pc.ContactType = 'P'

                        SELECT
                            t.TLevelId,
                            CASE 
                            WHEN (t.TLevelStatus = 1 and pc.ContactType = 'P' and t.CreatedOn > @CutOffDate) THEN 1
                            WHEN (t.TLevelStatus = 1 and pc.ContactType = 'P' and ((t.CreatedOn < @CutOffDate and t.UpdatedOn > @CutOffDate) or (t.CreatedOn < @CutOffDate and v.UpdatedOn > @CutOffDate))) THEN 2
                            WHEN (t.TLevelStatus = 2 and t.UpdatedOn > @CutOffDate) THEN 3
                            END AS UpdateType,
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
                        WHERE 
                        (t.TLevelStatus = 1 and pc.ContactType = 'P' and t.CreatedOn > @CutOffDate) 
                        OR (t.TLevelStatus = 1 and pc.ContactType = 'P' and ((t.CreatedOn < @CutOffDate and t.UpdatedOn > @CutOffDate) or (t.CreatedOn < @CutOffDate and v.UpdatedOn > @CutOffDate))) 
                        OR (t.TLevelStatus = 2 and t.UpdatedOn > @CutOffDate)
                        order by TLevelId
                        OFFSET (@PageNumber-1)*@PageSize ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

            var paramz = new
            {
                query.CutOffDate,
                query.PageNumber,
                query.PageSize
            };

            using var queryReader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);
            var courseCount = (await queryReader.ReadFirstOrDefaultAsync<int>());
            var tLevels = (await queryReader.ReadAsync<TLevelUpdateItem>()).ToList();

            var listOfTLevels = new ListOfTLevelUpdates()
            {
                TLevelCount = courseCount,
                TLevels = tLevels
            };
            return listOfTLevels;
        }
    }
}
