using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CourseStarteDateBulkUpdate : ISqlQuery<Success>
    {

        public Guid ProviderId { get; set; }

        public Guid[] SelectedCourseRunid { get; set; }
        public DateTime? StartDate { get; set; }

    }
  
}
