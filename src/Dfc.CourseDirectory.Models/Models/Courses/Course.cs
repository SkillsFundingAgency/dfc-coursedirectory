using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class Course : ICourse // ValueObject<Course>, ICourse
    {
        public Guid id { get; set; }
        public QuAP QuAP { get; set; }
        public CourseData CourseData { get; set; }
        public IEnumerable<CourseRun> CourseRun { get; set; }

        //public Course(
        //    Guid id,
        //    QuAP quAP,
        //    CourseData data,
        //    IEnumerable<CourseRun> run)
        //{
        //    Throw.IfNull(id, nameof(id));
        //    Throw.IfNull(quAP, nameof(quAP));
        //    Throw.IfNull(data, nameof(data));
        //    Throw.IfNull(data, nameof(run));

        //    ID = id;
        //    QuAP = quAP;
        //    CourseData = data;
        //}

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    yield return ID;
        //    yield return QuAP;
        //    yield return CourseData;
        //}
    }
}