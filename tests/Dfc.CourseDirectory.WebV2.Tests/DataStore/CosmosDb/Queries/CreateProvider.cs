﻿using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries
{
    public class CreateProvider : ICosmosDbQuery<CreateProviderResult>
    {
        public Guid ProviderId { get; set; }
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
        public int Ukprn { get; set; }
        public string Alias { get; set; }
        public string CourseDirectoryName { get; set; }
        public string MarketingInformation { get; set; }
    }

    public enum CreateProviderResult { Ok }
}
