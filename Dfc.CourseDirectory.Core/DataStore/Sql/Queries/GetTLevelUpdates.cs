using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetTLevelUpdates : ISqlQuery<ListOfTLevelUpdates>
    {
        public DateTime CutOffDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
