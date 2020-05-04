using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries
{
    public class CreateStandard : ICosmosDbQuery<Success>
    {
        public Guid Id { get; set; }
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string StandardName { get; set; }
        public string NotionalEndLevel { get; set; }
        public string OtherBodyApprovalRequired { get; set; }
    }
}
