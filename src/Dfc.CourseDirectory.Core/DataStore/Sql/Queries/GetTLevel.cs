using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetTLevel : ISqlQuery<TLevel>
    {
        public Guid TLevelId { get; set; }
    }
}
