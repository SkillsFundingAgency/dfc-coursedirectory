using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel
{
    public class EditTLevelJourneyModelFactory
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public EditTLevelJourneyModelFactory(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<EditTLevelJourneyModel> CreateModel(Guid tLevelId)
        {
            var tLevel = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevel() { TLevelId = tLevelId });

            if (tLevel == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.TLevel, tLevelId);
            }

            return new EditTLevelJourneyModel()
            {
                EntryRequirements = tLevel.EntryRequirements,
                HowYoullBeAssessed = tLevel.HowYoullBeAssessed,
                HowYoullLearn = tLevel.HowYoullLearn,
                LocationVenueIds = tLevel.Locations.Select(l => l.VenueId).ToArray(),
                StartDate = tLevel.StartDate,
                ProviderId = tLevel.ProviderId,
                TLevelId = tLevel.TLevelId,
                TLevelDefinitionId = tLevel.TLevelDefinition.TLevelDefinitionId,
                TLevelName = tLevel.TLevelDefinition.Name,
                Website = tLevel.Website,
                WhatYouCanDoNext = tLevel.WhatYouCanDoNext,
                WhatYoullLearn = tLevel.WhatYoullLearn,
                WhoFor = tLevel.WhoFor,
                YourReference = tLevel.YourReference,
                IsValid = true
            };
        }
    }
}
