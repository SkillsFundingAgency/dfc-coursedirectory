using System;
using System.Collections.Generic;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel
{
    [JourneyState]
    public class EditTLevelJourneyModel
    {
        public Guid TLevelId { get; set; }
        public Guid ProviderId { get; set; }
        public Guid TLevelDefinitionId { get; set; }
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
        public bool IsValid { get; set; }
    }
}
