using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CourseData : ValueObject<CourseData>, ICourseData
    {
        public Venue Venue { get; }
        public CourseInformation Information { get; }

        public CourseData(
            Venue venue,
            CourseInformation info)
        {
            Throw.IfNull(venue, nameof(venue));
            Throw.IfNull(info, nameof(info));

            Venue = venue;
            Information = info;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Venue;
            yield return Information;
        }
    }
}