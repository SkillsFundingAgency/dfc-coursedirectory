using Dfc.CourseDirectory.Models.Models.Providers;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public  class CsvDfcMigrationReport
    {
        [JsonProperty(PropertyName ="Provider")]
        public string ProviderName { get; set; }

        [JsonProperty(PropertyName = "Provider_Type")]
        public ProviderType ProviderType { get; set; }

        [JsonProperty(PropertyName = "Migration_Date")]
        public string MigrationDate { get; set; }

        [JsonProperty(PropertyName = "Migrated")]
        public int? MigratedCount { get; set; }

        [JsonProperty(PropertyName = "Not_Migrated")]
        public int? FailedMigrationCount { get; set; }

        [JsonProperty(PropertyName = "Errors")]
        public int MigrationPendingCount { get; set; }

        [JsonProperty(PropertyName = "Live")]
        public int LiveCount { get; set; }

        [JsonProperty(PropertyName = "Migration_Rate")]
        public decimal MigrationRate { get; set; }
    }
}
