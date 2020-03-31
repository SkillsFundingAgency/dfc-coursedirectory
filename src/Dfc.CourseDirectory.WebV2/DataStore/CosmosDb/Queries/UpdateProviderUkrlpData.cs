using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class UpdateProviderUkrlpData : ICosmosDbQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public OneOf<None, string> UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
