using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CourseText : ValueObject<CourseText>, ICourseText
    {
        public string CourseTitle { get; set; }
        public string Learn { get; set; }
        public string How { get; set; }
        public string Why { get; set; }

        public CourseText(
            string courseTitle,
            string learn,
            string how,
            string why)
        {
            Throw.IfNullOrWhiteSpace(courseTitle, nameof(courseTitle));
            Throw.IfNullOrWhiteSpace(learn, nameof(learn));
            Throw.IfNullOrWhiteSpace(how, nameof(how));
            Throw.IfNullOrWhiteSpace(why, nameof(why));

            CourseTitle = courseTitle;
            Learn = learn;
            How = how;
            Why = why;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CourseTitle;
            yield return Learn;
            yield return How;
            yield return Why;
        }
    }
}