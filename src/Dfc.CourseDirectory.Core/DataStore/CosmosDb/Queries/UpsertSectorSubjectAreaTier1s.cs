using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpsertSectorSubjectAreaTier1s : ICosmosDbQuery<None>
    {
        public DateTime Now { get; set; }
        public IEnumerable<UpsertSectorSubjectAreaTier1sRecord> Records { get; set; }
    }

    public class UpsertSectorSubjectAreaTier1sRecord
    {
        public decimal SectorSubjectAreaTier1Id { get; set; }
        public string SectorSubjectAreaTier1Desc { get; set; }
        public string SectorSubjectAreaTier1Desc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
