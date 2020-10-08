using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ReallocateDuplicateApprenticeshipLocationIds : ICosmosDbQuery<None>
    {
        public Apprenticeship Apprenticeship { get; set; }
        public IEnumerable<Guid> DuplicateLocationIds { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }
}
