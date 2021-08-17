using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Apprenticeship
    {
        public Guid ApprenticeshipId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUkprn { get; set; }
        public Standard Standard { get; set; }
        public string MarketingInformation { get; set; }
        public string ApprenticeshipWebsite { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IReadOnlyCollection<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }
    }
}
