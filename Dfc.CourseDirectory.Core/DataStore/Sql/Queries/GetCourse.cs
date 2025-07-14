using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourse : ISqlQuery<Course>
    {
        public Guid CourseId { get; set; }
    }
}
