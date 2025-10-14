using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsSectorSubjectAreaTier2s : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsSectorSubjectAreaTier2sRecord> Records { get; set; }
    }

    public class UpsertLarsSectorSubjectAreaTier2sRecord
    {
        public string SectorSubjectAreaTier2 { get; set; }
        public string SectorSubjectAreaTier2Desc { get; set; }
        public string SectorSubjectAreaTier2Desc2 { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
