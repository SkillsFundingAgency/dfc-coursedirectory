using System;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CourseRow
    {
        [Index(0), Name("LARS_QAN")]
        public string LarsQan { get; set; }

        [Index(1), Name("WHO_IS_THIS_COURSE_FOR")]
        public string WhoIsThisCourseFor { get; set; }

        [Index(2), Name("ENTRY_REQUIREMENTS")]
        public string EntryRequirements { get; set; }


        public static CourseRow FromModel(Course course) => new CourseRow()
        {
            LarsQan = course.LarsQan,
            WhoIsThisCourseFor = course.WhoIsThisCourseFor,
            EntryRequirements = course.EntryRequirements,
        };
    }
}
