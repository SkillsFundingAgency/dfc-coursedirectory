using System;
using Dfc.CourseDirectory.Services.Models.Providers;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services.Models.Courses
{
    public  class CsvDfcMigrationReport
    {
        [JsonProperty(PropertyName ="Provider")]
        public string ProviderName { get; set; }
         
        [JsonProperty(PropertyName = "UKPRN")]
        public string UKPRN { get; set; }
        
        [JsonProperty(PropertyName = "Provider_Type")]
        public ProviderType ProviderType { get; set; }

        [JsonProperty(PropertyName = "Migration_Date")]
        public string MigrationDate { get; set; }

        [JsonProperty(PropertyName = "Migrated")]
        public int? MigratedCount { get; set; }

        [JsonProperty(PropertyName = "Errors")]
        public int? Errors { get; set; }

        [JsonProperty(PropertyName = "Not_Migrated")]
        public int? FailedMigrationCount { get; set; }

        [JsonProperty(PropertyName = "Migration_Pending")]
        public int MigrationPendingCount { get; set; }

        [JsonProperty(PropertyName = "Live")]
        public int LiveCount { get; set; }

        [JsonProperty(PropertyName = "Migration_Rate")]
        public decimal MigrationRate { get; set; }

        [JsonProperty(PropertyName = "Created_Date")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty(PropertyName = "Created_By")]
        public string CreatedBy { get; set; }
    }
}
