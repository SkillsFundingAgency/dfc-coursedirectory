using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertProviderUploadRowsRecord
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastValidated { get; set; }
        public int Ukprn { get; set; }
        public int ProviderStatus { get; set; }
        public int ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string TradingName { get; set; }
    }
}
