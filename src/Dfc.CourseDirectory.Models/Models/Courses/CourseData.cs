using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CourseData : ICourseData // ValueObject<CourseData>, ICourseData
    {
        public Guid ID { get; set; }
        public Guid CourseID { get; set; }
        public string CourseTitle { get; set; }

        //public CourseData(
        //    Guid id,
        //    Guid courseID,
        //    string courseTitle)
        //{
        //    Throw.IfNull(id, nameof(id));
        //    Throw.IfNull(id, nameof(courseID));
        //    Throw.IfNullOrWhiteSpace(courseTitle, nameof(courseTitle));

        //    ID = id;
        //    CourseID = courseID;
        //    CourseTitle = courseTitle;
        //}
        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    yield return ID;
        //    yield return CourseID;
        //    yield return CourseTitle;

        //}
    }
}
