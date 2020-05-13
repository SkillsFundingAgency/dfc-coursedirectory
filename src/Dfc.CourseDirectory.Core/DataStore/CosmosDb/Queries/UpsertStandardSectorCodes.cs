using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpsertStandardSectorCodes : ICosmosDbQuery<None>
    {
        public DateTime Now { get; set; }
        public IEnumerable<UpsertStandardSectorCodesRecord> Records { get; set; }
    }

    public class UpsertStandardSectorCodesRecord
    {
        public string StandardSectorCodeId { get; set; }
        public string StandardSectorCodeDesc { get; set; }
        public string StandardSectorCodeDesc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
