using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelHandler : ISqlQueryHandler<GetTLevel, TLevel>
    {
        public async Task<TLevel> Execute(SqlTransaction transaction, GetTLevel query)
        {
            var sql = @"
SELECT
    t.TLevelId, t.TLevelStatus, p.ProviderId, p.ProviderName,
    t.WhoFor, t.EntryRequirements, t.WhatYoullLearn, t.HowYoullLearn, t.HowYoullBeAssessed,
    t.WhatYouCanDoNext, t.YourReference, t.StartDate, t.Website, t.CreatedOn, t.UpdatedOn,
    d.TLevelDefinitionId, d.FrameworkCode, d.ProgType, d.Name
FROM Pttcd.TLevels t
JOIN Pttcd.TLevelDefinitions d ON t.TLevelDefinitionId = d.TLevelDefinitionId
JOIN Pttcd.Providers p ON t.ProviderId = p.ProviderId
WHERE t.TLevelId = @TLevelId AND t.TLevelStatus = @LiveTLevelStatus

SELECT tll.TLevelLocationId, tll.TLevelLocationStatus, tll.VenueId, v.VenueName
FROM Pttcd.TLevelLocations tll
JOIN Pttcd.Venues v ON tll.VenueId = v.VenueId
WHERE TLevelId = @TLevelId
AND TLevelLocationStatus = @LiveTLevelLocationStatus
ORDER BY v.VenueName";

            var param = new
            {
                query.TLevelId,
                LiveTLevelStatus = TLevelStatus.Live,
                LiveTLevelLocationStatus = TLevelLocationStatus.Live
            };

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, param, transaction: transaction))
            {
                var tLevel = reader.Read<TLevel, TLevelDefinition, TLevel>(
                    (tl, definition) =>
                    {
                        tl.TLevelDefinition = definition;
                        return tl;
                    },
                    splitOn: "TLevelDefinitionId")
                    .SingleOrDefault();

                if (tLevel == null)
                {
                    return null;
                }

                tLevel.Locations = reader.Read<TLevelLocation>().AsList();

                return tLevel;
            }
        }
    }
}
