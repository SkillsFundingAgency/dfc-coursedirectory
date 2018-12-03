using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using Dfc.CourseDirectory.Models.Models.Venues;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class Course : ValueObject<Course>, ICourse
    {
        public Provider Provider { get; set; }
        public Qualification Qualification { get; }
        public Venue Venue { get; }

        public Course(
            Provider provider,
            Qualification qualification,
            Venue venue)
        {

        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Provider;
            yield return Qualification;
            yield return Venue;
        }
    }
}