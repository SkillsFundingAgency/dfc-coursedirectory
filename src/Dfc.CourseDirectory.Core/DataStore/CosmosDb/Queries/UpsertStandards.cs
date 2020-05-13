using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class UpsertStandards : ICosmosDbQuery<None>
    {
        public IEnumerable<UpsertStandardsRecord> Records { get; set; }
    }

    public class UpsertStandardsRecord
    {
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string StandardName { get; set; }
        public string NotionalEndLevel { get; set; }
        public string OtherBodyApprovalRequired { get; set; }
    }
}
