using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetStandards : ISqlQuery<IDictionary<(int StandardCode, int StandardVersion), Standard>>
    {
        public IEnumerable<(int StandardCode, int StandardVersion)> StandardCodes { get; set; }
    }
}
