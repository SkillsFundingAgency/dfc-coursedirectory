using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class UpdateProviderMarketingInfo : ICosmosDbQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public string MarketingInformation { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}