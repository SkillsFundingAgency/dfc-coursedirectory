using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderById : ISqlQuery<Provider>
    {
        public Guid ProviderId { get; set; }
    }
}
