using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries
{
    public class CreateFramework : ICosmosDbQuery<Success>
    {
        public Guid Id { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public int PathwayCode { get; set; }
        public string NasTitle { get; set; }
    }
}
