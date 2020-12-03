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
            string whoFor,
            string entryRequirements,
            string whatYoullLearn,
            string howYoullLearn,
            string howYoullBeAssessed,
            string whatYouCanDoNext,
            string yourReference,
            DateTime startDate,
            IReadOnlyCollection<Guid> locationVenueIds,
            string website,
            UserInfo createdBy,
            DateTime? createdOn = null)
        {
            var tLevelId = Guid.NewGuid();

            if (locationVenueIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one location venue ID must be specified.",
                    nameof(locationVenueIds));
            }

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
                    StartDate = startDate,
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
