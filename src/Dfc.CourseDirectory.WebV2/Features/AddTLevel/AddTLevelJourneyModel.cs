using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2.Features.AddTLevel
{
    [JourneyState]
    public class AddTLevelJourneyModel
    {
        public AddTLevelJourneyCompletedStages ValidStages { get; set; }
        public Guid? TLevelId { get; set; }
        public Guid? TLevelDefinitionId { get; set; }
        public string TLevelName { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string YourReference { get; set; }
        public DateTime? StartDate { get; set; }
        public IList<Guid> LocationVenueIds { get; set; }
        public string Website { get; set; }

        public void SetTLevel(
            Guid tLevelDefinitionId,
            string tLevelName,
            TLevelDefinitionExemplarContent exemplarContent = null)
        {
            if (tLevelDefinitionId != TLevelDefinitionId)
            {
                // If the T Level has been changed, reset the state for subsequent stages.
                // The `isComplete` false value prevents users jumping ahead to later parts of the journey
                // without explicitly submitting at each of these stages.

                SetDescription(
                    exemplarContent?.WhoFor,
                    exemplarContent?.EntryRequirements,
                    exemplarContent?.WhatYoullLearn,
                    exemplarContent?.HowYoullLearn,
                    exemplarContent?.HowYoullBeAssessed,
                    exemplarContent?.WhatYouCanDoNext,
                    isComplete: false);

                SetDetails(null, null, null, null, isComplete: false);
            }

            TLevelDefinitionId = tLevelDefinitionId;
            TLevelName = tLevelName;
            ValidStages |= AddTLevelJourneyCompletedStages.SelectTLevel;
        }

        public void SetDescription(
            string whoFor,
            string entryRequirements,
            string whatYoullLearn,
            string howYoullLearn,
            string howYoullBeAssessed,
            string whatYouCanDoNext,
            bool isComplete)
        {
            WhoFor = whoFor;
            EntryRequirements = entryRequirements;
            WhatYoullLearn = whatYoullLearn;
            HowYoullLearn = howYoullLearn;
            HowYoullBeAssessed = howYoullBeAssessed;
            WhatYoullLearn = whatYoullLearn;
            WhatYouCanDoNext = whatYouCanDoNext;

            if (isComplete)
            {
                ValidStages |= AddTLevelJourneyCompletedStages.Description;
            }
            else
            {
                ValidStages &= ~AddTLevelJourneyCompletedStages.Description;
            }
        }

        public void SetDetails(
            string yourReference,
            DateTime? startDate,
            IEnumerable<Guid> locationVenueIds,
            string website,
            bool isComplete)
        {
            YourReference = yourReference;
            StartDate = startDate;
            LocationVenueIds = locationVenueIds?.ToArray() ?? Array.Empty<Guid>();
            Website = website;

            if (isComplete)
            {
                ValidStages |= AddTLevelJourneyCompletedStages.Details;
            }
            else
            {
                ValidStages &= ~AddTLevelJourneyCompletedStages.Details;
            }
        }

        public void SetCreatedTLevel(Guid tLevelId)
        {
            TLevelId = tLevelId;
        }
    }

    [Flags]
    public enum AddTLevelJourneyCompletedStages
    {
        None = 0,
        SelectTLevel = 1,
        Description = 2,
        Details = 4,
        All = SelectTLevel | Description | Details
    }
}
