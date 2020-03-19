using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries
{
    public class CreateFramework : ICosmosDbQuery<Success>
    {
        public string Id { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public int PathwayCode { get; set; }
        public string NasTitle { get; set; }
    }
}
