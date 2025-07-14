using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetPostcodeInfos : ISqlQuery<IDictionary<string, PostcodeInfo>>
    {
        public IEnumerable<string> Postcodes { get; set; }
    }
}
