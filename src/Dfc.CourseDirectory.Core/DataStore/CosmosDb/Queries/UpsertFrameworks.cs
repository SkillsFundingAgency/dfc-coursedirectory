using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpsertFrameworks : ICosmosDbQuery<None>
    {
        public DateTime Now { get; set; }
        public IEnumerable<UpsertFrameworksRecord> Records { get; set; }
    }

    public class UpsertFrameworksRecord
    {
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public int PathwayCode { get; set; }
        public string PathwayName { get; set; }
        public string NasTitle { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public decimal SectorSubjectAreaTier1 { get; set; }
        public decimal SectorSubjectAreaTier2 { get; set; }
        public int RecordStatusId { get; set; }
    }
}
