using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries
{
    public class CreateProvider : ICosmosDbQuery<CreateProviderResult>
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public string MarketingInformation { get; set; }
        public string CourseDirectoryName { get; set; }
        public string Alias { get; set; }
        public IEnumerable<ProviderContact> ProviderContact { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? BulkUploadInProgress { get; set; }
    }

    public enum CreateProviderResult { Ok }
}
