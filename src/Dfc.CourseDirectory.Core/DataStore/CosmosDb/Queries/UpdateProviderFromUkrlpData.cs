using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderFromUkrlpData : ICosmosDbQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public string Alias { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public IEnumerable<ProviderAlias> Aliases { get; set; }
        public IEnumerable<ProviderContact> Contacts { get; set; }
    }
}
