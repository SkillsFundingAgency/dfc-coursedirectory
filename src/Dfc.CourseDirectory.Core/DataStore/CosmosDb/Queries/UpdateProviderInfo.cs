using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
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
