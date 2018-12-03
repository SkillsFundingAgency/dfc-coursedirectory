using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class Course : ValueObject<Course>, ICourse
    {
        public Provider Provider { get; }
        public Qualification Qualification { get; }
        public CourseData CourseData { get; }
        public CourseText CourseText { get; }

        public Course(
            Provider provider,
            Qualification qualification,
            CourseData data,
            CourseText text)
        {
            Throw.IfNull(provider, nameof(provider));
            Throw.IfNull(qualification, nameof(qualification));
            Throw.IfNull(data, nameof(data));
            Throw.IfNull(text, nameof(text));

            Provider = provider;
            Qualification = qualification;
            CourseData = data;
            CourseText = text;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Provider;
            yield return Qualification;
            yield return CourseData;
            yield return CourseText;
        }
    }
}