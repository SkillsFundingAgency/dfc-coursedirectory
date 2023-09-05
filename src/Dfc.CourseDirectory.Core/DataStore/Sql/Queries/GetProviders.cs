using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviders : ISqlQuery<IList<ProviderUkprn>>
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Chunk { get; set; }
    }
}
