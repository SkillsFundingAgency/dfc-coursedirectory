using System;
using Dfc.CourseDirectory.WebV2.Security;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class UpdateProviderInfo : ICosmosDbQuery<OneOf<Success, NotFound>>
    {
        public Guid ProviderId { get; set; }
        public string Alias { get; set; }
        public OneOf<string, None> BriefOverview { get; set; }
        public OneOf<string, None> CourseDirectoryName { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
