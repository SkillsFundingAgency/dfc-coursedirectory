using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class Course : ValueObject<Course>, ICourse
    {
        public Guid ID { get; }
        public QuAP QuAP { get; }
        public CourseData CourseData { get; }
        public IEnumerable<CourseRun> CourseRun { get; }

        public Course(
            Guid id,
            QuAP quAP,
            CourseData data,
            IEnumerable<CourseRun> run)
        {
            Throw.IfNull(id, nameof(id));
            Throw.IfNull(quAP, nameof(quAP));
            Throw.IfNull(data, nameof(data));
            Throw.IfNull(data, nameof(run));

            ID = id;
            QuAP = quAP;
            CourseData = data;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ID;
            yield return QuAP;
            yield return CourseData;
        }
    }
}