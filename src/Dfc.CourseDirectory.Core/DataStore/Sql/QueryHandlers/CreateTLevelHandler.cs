using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateTLevelHandler : ISqlQueryHandler<CreateTLevel, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateTLevel query)
        {
            var sql = $@"
INSERT INTO Pttcd.TLevels (
    TLevelId, TLevelStatus, CreatedOn, CreatedByUserId, UpdatedOn, UpdatedByUserId, ProviderId,
    TLevelDefinitionId, WhoFor, EntryRequirements, WhatYoullLearn, HowYoullLearn, HowYoullBeAssessed,
    WhatYouCanDoNext, YourReference, StartDate, Website
) VALUES (
    @TLevelId, @TLevelStatus, @CreatedOn, @CreatedByUserId, @UpdatedOn, @UpdatedByUserId, @ProviderId,
    @TLevelDefinitionId, @WhoFor, @EntryRequirements, @WhatYoullLearn, @HowYoullLearn, @HowYoullBeAssessed,
    @WhatYouCanDoNext, @YourReference, @StartDate, @Website
)

INSERT INTO Pttcd.TLevelLocations (TLevelLocationId, TLevelId, VenueId, TLevelLocationStatus)
SELECT NEWID(), @TLevelId, Id, @TLevelLocationStatus
FROM @LocationVenueIds";

            await transaction.Connection.ExecuteAsync(
                sql,
                param: new
                {
                    query.TLevelId,
                    TLevelStatus = TLevelStatus.Live,
                    CreatedOn = query.CreatedOn,
                    CreatedByUserId = query.CreatedBy.UserId,
                    UpdatedOn = query.CreatedOn,
                    UpdatedByUserId = query.CreatedBy.UserId,
                    query.ProviderId,
                    query.TLevelDefinitionId,
                    query.WhoFor,
                    query.EntryRequirements,
                    query.WhatYoullLearn,
                    query.HowYoullLearn,
                    query.HowYoullBeAssessed,
                    query.WhatYouCanDoNext,
                    query.YourReference,
                    query.StartDate,
                    query.Website,
                    LocationVenueIds = TvpHelper.CreateGuidIdTable(query.LocationVenueIds),
                    TLevelLocationStatus = TLevelLocationStatus.Live
                },
                transaction: transaction);

            return new Success();
        }
    }
}
