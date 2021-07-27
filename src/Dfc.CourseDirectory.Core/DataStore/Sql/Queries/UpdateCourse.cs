using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateCourse : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid CourseId { get; set; }
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
