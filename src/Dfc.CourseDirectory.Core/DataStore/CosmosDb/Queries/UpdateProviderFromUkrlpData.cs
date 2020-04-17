using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpdateProviderFromUkrlpData : ICosmosDbQuery<Success>
    {
        public Guid Id { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public string Alias { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public List<ProviderContact> ProviderContact { get; set; }

    }
}
