using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class CreateProviderFromUkrlpData : ICosmosDbQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public ProviderStatus Status { get; set; }
        public int Ukprn { get; set; }
        public string Alias { get; set; }
        public ProviderType ProviderType { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public IEnumerable<ProviderContact> ProviderContact { get; set; }
    }
}
