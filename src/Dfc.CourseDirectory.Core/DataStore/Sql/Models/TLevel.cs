using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class TLevel
    {
        public Guid TLevelId { get; set; }
        public TLevelStatus TLevelStatus { get; set; }
        public TLevelDefinition TLevelDefinition { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string YourReference { get; set; }
        public DateTime StartDate { get; set; }
        public IReadOnlyList<TLevelLocation> Locations { get; set; }
        public string Website { get; set; }
    }
}
