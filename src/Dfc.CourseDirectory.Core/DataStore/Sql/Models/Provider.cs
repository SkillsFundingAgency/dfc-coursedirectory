using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Provider
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string Alias { get; set; }
        public ProviderDisplayNameSource DisplayNameSource { get; set; }

        public string DisplayName => DisplayNameSource switch
        {
            ProviderDisplayNameSource.ProviderName => ProviderName,
            ProviderDisplayNameSource.TradingName => Alias,
            _ => throw new NotSupportedException($"Unknown {nameof(DisplayNameSource)}: '{DisplayNameSource}'.")
        };

        public bool HaveAlias => !string.IsNullOrEmpty(Alias);
    }
}
