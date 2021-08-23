using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateApprenticeship : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid Id { get; set; }
        public int ProviderUkprn { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Core.Models.Standard Standard { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IEnumerable<CreateApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public int? Status { get; set; }
        public DateTime UpdatedDate { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public IEnumerable<BulkUploadError> BulkUploadErrors { get; set; }
    }
}
