using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetFeChoicesByProviderUkprns : ISqlQuery<IReadOnlyDictionary<int, FeChoice>>
    {
        public IEnumerable<int> ProviderUkprns { get; set; }
    }
}
