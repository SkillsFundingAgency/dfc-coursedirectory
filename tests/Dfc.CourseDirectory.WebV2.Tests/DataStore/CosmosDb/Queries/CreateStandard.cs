using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries
{
    public class CreateStandard : ICosmosDbQuery<Success>
    {
        public string Id { get; set; }
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string StandardName { get; set; }
        public string NotionalEndLevel { get; set; }
        public string OtherBodyApprovalRequired { get; set; }
    }
}
