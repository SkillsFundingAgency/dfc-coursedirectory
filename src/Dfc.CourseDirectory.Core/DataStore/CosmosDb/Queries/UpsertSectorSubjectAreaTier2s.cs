using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpsertSectorSubjectAreaTier2s : ICosmosDbQuery<None>
    {
        public DateTime Now { get; set; }
        public IEnumerable<UpsertSectorSubjectAreaTier2sRecord> Records { get; set; }
    }

    public class UpsertSectorSubjectAreaTier2sRecord
    {
        public decimal SectorSubjectAreaTier2Id { get; set; }
        public string SectorSubjectAreaTier2Desc { get; set; }
        public string SectorSubjectAreaTier2Desc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
