using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsSectorSubjectAreaTier1s : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsSectorSubjectAreaTier1sRecord> Records { get; set; }
    }

    public class UpsertLarsSectorSubjectAreaTier1sRecord
    {
        public string SectorSubjectAreaTier1 { get; set; }
        public string SectorSubjectAreaTier1Desc { get; set; }
        public string SectorSubjectAreaTier1Desc2 { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
