using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Provider
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
        public ProviderType ProviderType { get; set; }
        public string Alias { get; set; }
        public ProviderDisplayNameSource DisplayNameSource { get; set; }
        public decimal? LearnerSatisfaction { get; set; }
        public decimal? EmployerSatisfaction { get; set; }

        public string MarketingInformation { get; set; }

        public string ProviderStatus { get; set; }
        public string CourseDirectoryName { get; set; }



        public string DisplayName => DisplayNameSource switch
        {
            ProviderDisplayNameSource.TradingName => HaveAlias ? Alias : ProviderName,
            ProviderDisplayNameSource.ProviderName => ProviderName,
            _ => throw new NotSupportedException($"Unknown {nameof(DisplayNameSource)}: '{DisplayNameSource}'.")
        };

        public bool HaveAlias => !string.IsNullOrEmpty(Alias);
    }
}
