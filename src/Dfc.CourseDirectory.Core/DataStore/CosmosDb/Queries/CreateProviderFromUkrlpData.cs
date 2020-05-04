using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using Newtonsoft.Json.Linq;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class CreateProviderFromUkrlpData : ICosmosDbQuery<Success>
    {
        public Guid Id { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        public string Alias { get; set; }
        public ProviderType ProviderType { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public List<ProviderContact> ProviderContact { get; set; }
    }
}
