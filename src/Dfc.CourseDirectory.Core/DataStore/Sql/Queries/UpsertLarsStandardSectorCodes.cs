using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsStandardSectorCodes : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsStandardSectorCodeRecord> Records { get; set; }
    }

    public class UpsertLarsStandardSectorCodeRecord
    {
        public int StandardSectorCode { get; set; }
        public string StandardSectorCodeDesc { get; set; }
        public string StandardSectorCodeDesc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
