using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<TLevel> CreateTLevel(
            Guid providerId,
            Guid tLevelDefinitionId,
            IReadOnlyCollection<Guid> locationVenueIds,
            UserInfo createdBy,
            DateTime? startDate = null,
            string whoFor = "Who this T Level is for",
            string entryRequirements = null,
            string whatYoullLearn = null,
            string howYoullLearn = null,
            string howYoullBeAssessed = null,
            string whatYouCanDoNext = null,
            string yourReference = null,
            string website = null,
            DateTime? createdOn = null)
        {
            var tLevelId = Guid.NewGuid();

            if (locationVenueIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one location venue ID must be specified.",
                    nameof(locationVenueIds));
            }

            startDate ??= new DateTime(2021, 10, 1);

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                await dispatcher.ExecuteQuery(new CreateTLevel()
                {
                    TLevelId = tLevelId,
                    ProviderId = providerId,
                    TLevelDefinitionId = tLevelDefinitionId,
                    WhoFor = whoFor,
                    EntryRequirements = entryRequirements,
                    WhatYoullLearn = whatYoullLearn,
                    HowYoullLearn = howYoullLearn,
                    HowYoullBeAssessed = howYoullBeAssessed,
                    WhatYouCanDoNext = whatYouCanDoNext,
                    YourReference = yourReference,
                    StartDate = startDate.Value,
                    LocationVenueIds = locationVenueIds,
                    Website = website,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn ?? _clock.UtcNow
                });

                return await dispatcher.ExecuteQuery(new GetTLevel() { TLevelId = tLevelId });
            });
        }
    }
}
