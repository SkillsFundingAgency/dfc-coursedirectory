using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class UpdateProviderInfo : ICosmosDbQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public OneOf<None, string> Alias { get; set; }
        public OneOf<None, string> MarketingInformation { get; set; }
        public OneOf<None, string> CourseDirectoryName { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
