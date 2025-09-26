using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderUploadRow
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IReadOnlyCollection<string> Errors { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastValidated { get; set; }
        public string Ukprn { get; set; }
        public int ProviderStatus { get; set; }
        public int ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string TradingName { get; set; } 

    }
}
