using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateTLevelHandler : ISqlQueryHandler<UpdateTLevel, OneOf<NotFound, UpdateTLevelFailedReason, Success>>
    {
        public async Task<OneOf<NotFound, UpdateTLevelFailedReason, Success>> Execute(
            SqlTransaction transaction,
            UpdateTLevel query)
        {
            var sql = $@"
DECLARE @Result INT

UPDATE Pttcd.TLevels SET
    UpdatedOn = @UpdatedOn,
    UpdatedByUserId = @UpdatedByUserId,
    WhoFor = @WhoFor,
    EntryRequirements = @EntryRequirements,
    WhatYoullLearn = @WhatYoullLearn,
    HowYoullLearn = @HowYoullLearn,
    HowYoullBeAssessed = @HowYoullBeAssessed,
    WhatYouCanDoNext = @WhatYouCanDoNext,
    YourReference = @YourReference,
    StartDate = @StartDate,
    Website = @Website
WHERE TLevelId = @TLevelId
AND TLevelStatus = @LiveTLevelStatus

SELECT @Result = @@ROWCOUNT

MERGE Pttcd.TLevelLocations AS target
USING (SELECT Id VenueId FROM @LocationVenueIds) AS source
ON target.VenueId = source.VenueId AND target.TLevelId = @TLevelId AND target.TLevelLocationStatus = @LiveTLevelLocationStatus
WHEN NOT MATCHED THEN
    INSERT (TLevelLocationId, TLevelLocationStatus, TLevelId, VenueId) VALUES (NEWID(), @LiveTLevelLocationStatus, @TLevelId, source.VenueId)
WHEN NOT MATCHED BY SOURCE AND target.TLevelId = @TLevelId THEN
    UPDATE SET TLevelLocationStatus = @DeletedTLevelLocationStatus;

DECLARE @TLevelIds Pttcd.GuidIdTable

INSERT INTO @TLevelIds VALUES (@TLevelId)

EXEC Pttcd.RefreshFindACourseIndexForTLevels @TLevelIds, @UpdatedOn

SELECT @Result";

            try
            {
                var result = await transaction.Connection.QuerySingleAsync<Result>(
                    sql,
                    param: new
                    {
                        query.TLevelId,
                        query.UpdatedOn,
                        UpdatedByUserId = query.UpdatedBy.UserId,
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
                        LiveTLevelStatus = TLevelStatus.Live,
                        LiveTLevelLocationStatus = TLevelLocationStatus.Live,
                        DeletedTLevelLocationStatus = TLevelLocationStatus.Deleted
                    },
                    transaction: transaction);

                if (result == Result.Ok)
                {
                    return new Success();
                }
                else
                {
                    return new NotFound();
                }
            }
            catch (SqlException ex) when (ex.Number == 2601 && ex.Message.Contains("'IX_TLevels_DefinitionProviderStartDate'"))
            {
                return UpdateTLevelFailedReason.TLevelAlreadyExistsForDate;
            }
        }

        private enum Result
        {
            Ok = 0,
            NotFound = 1
        }
    }
}
