using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsLearnAimRefTypes : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsLearnAimRefTypesRecord> Records { get; set; }
    }

    public class UpsertLarsLearnAimRefTypesRecord
    {
        public string LearnAimRefType { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public string LearnAimRefTypeDesc2 { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
