using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelsHandler : ISqlQueryHandler<GetTLevels, IReadOnlyCollection<TLevel>>
    {
        public async Task<IReadOnlyCollection<TLevel>> Execute(SqlTransaction transaction, GetTLevels query)
        {
            var sql = @$"
SELECT      t.TLevelId, t.TLevelStatus, p.ProviderId, p.ProviderName, t.WhoFor, t.EntryRequirements, t.WhatYoullLearn, t.HowYoullLearn, t.HowYoullBeAssessed, t.WhatYouCanDoNext, t.YourReference, t.StartDate, t.Website,
            d.TLevelDefinitionId, d.FrameworkCode, d.ProgType, d.QualificationLevel, d.Name
FROM        Pttcd.TLevels t
INNER JOIN  Pttcd.TLevelDefinitions d ON t.TLevelDefinitionId = d.TLevelDefinitionId
JOIN        Pttcd.Providers p ON t.ProviderId = p.ProviderId
WHERE       t.TLevelStatus = {(int)TLevelStatus.Live}

SELECT      tll.TLevelId, tll.TLevelLocationId, tll.TLevelLocationStatus, tll.VenueId, v.VenueName
FROM        Pttcd.TLevelLocations tll
INNER JOIN  Pttcd.TLevels t ON tll.TLevelId = t.TLevelId
INNER JOIN  Pttcd.Venues v ON tll.VenueId = v.VenueId
WHERE       t.TLevelStatus = {(int)TLevelStatus.Live}
AND         TLevelLocationStatus = {(int)TLevelLocationStatus.Live}
ORDER BY    v.VenueName";

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, transaction: transaction))
            {
                var tLevels = reader.Read<TLevel, TLevelDefinition, TLevel>(
                    (tl, definition) =>
                    {
                        tl.TLevelDefinition = definition;
                        return tl;
                    },
                    splitOn: nameof(TLevelDefinition.TLevelDefinitionId));

                var tLevelLocations = reader.Read<TLevelLocationRow>()
                    .ToLookup(r => r.TLevelId);

                return tLevels
                    .Select(tl =>
                    {
                        tl.Locations = tLevelLocations[tl.TLevelId].ToArray();
                        return tl;
                    })
                    .ToArray();
            }
        }

        private class TLevelLocationRow : TLevelLocation
        {
            public Guid TLevelId { get; set; }
        }
    }
}
