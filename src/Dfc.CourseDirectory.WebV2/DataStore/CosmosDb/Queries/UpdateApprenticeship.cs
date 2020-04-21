

using Dfc.CourseDirectory.WebV2.Models;
using OneOf.Types;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class UpdateApprenticeship : ICosmosDbQuery<Success>
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUkprn { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public StandardOrFramework StandardOrFramework { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IEnumerable<CreateApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserInfo CreatedByUser { get; set; }
    }
}
