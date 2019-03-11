
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseStatusCountResult : ValueObject<CourseStatusCountResult>, ICourseStatusCountResult
    {
        public int Status { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Status;
            yield return Description;
            yield return Count;
        }
    }
}
