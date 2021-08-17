using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateApprenticeship : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public Standard Standard { get; set; }
        public string MarketingInformation { get; set; }
        public string ApprenticeshipWebsite { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IEnumerable<CreateApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
