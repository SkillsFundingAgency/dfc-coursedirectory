using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class ProcessAllProviders : ISqlQuery<None>
    {
        public Func<IReadOnlyCollection<Provider>, Task> ProcessChunk { get; set; }
    }
}
