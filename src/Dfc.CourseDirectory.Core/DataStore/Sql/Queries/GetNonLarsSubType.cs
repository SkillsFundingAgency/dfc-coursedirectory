using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetNonLarsSubType : ISqlQuery<NonLarsSubType>
    {
        public Guid NonLarsSubTypeId { get; set; }

    }
}
