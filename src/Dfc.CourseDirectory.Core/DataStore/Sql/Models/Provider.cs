﻿using System;
using Dfc.CourseDirectory.Core.Models;

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

        public string MarketingInformation { get; set; }

        public string ProviderStatus { get; set; }

        public ProviderStatus Status { get; set; }
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
