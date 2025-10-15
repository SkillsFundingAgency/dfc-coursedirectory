using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateTLevel : ISqlQuery<OneOf<CreateTLevelFailedReason, Success>>
    {
        public Guid TLevelId { get; set; }
        public Guid ProviderId { get; set; }
        public Guid TLevelDefinitionId { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string YourReference { get; set; }
        public DateTime StartDate { get; set; }
        public IEnumerable<Guid> LocationVenueIds { get; set; }
        public string Website { get; set; }
        public UserInfo CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public enum CreateTLevelFailedReason
    {
        TLevelAlreadyExistsForDate
    }
}
