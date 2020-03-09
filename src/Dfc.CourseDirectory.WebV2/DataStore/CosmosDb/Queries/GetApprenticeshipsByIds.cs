using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetApprenticeshipsByIds : ICosmosDbQuery<IDictionary<Guid, Apprenticeship>>
    {
        public int Ukprn { get; set; }
        public IEnumerable<Guid> ApprenticeshipIds { get; set; }
    }
}
