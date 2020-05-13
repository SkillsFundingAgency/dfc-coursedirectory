using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpsertProgTypes : ICosmosDbQuery<None>
    {
        public DateTime Now { get; set; }
        public IEnumerable<UpsertProgTypesRecord> Records { get; set; }
    }

    public class UpsertProgTypesRecord
    {
        public int ProgTypeId { get; set; }
        public string ProgTypeDesc { get; set; }
        public string ProgTypeDesc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
