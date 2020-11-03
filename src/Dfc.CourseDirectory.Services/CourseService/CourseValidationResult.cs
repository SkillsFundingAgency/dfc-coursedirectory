using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Models.Interfaces.Courses;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseValidationResult
    {
        public ICourse Course{ get; set; }
        public IList<string> Issues { get; set; }
        public IList<CourseRunValidationResult> RunValidationResults { get; set; }
        public int TotalIssueCount { get { return Issues.Count() + RunValidationResults.SelectMany(c => c.Issues).Count(); } }

        public class CourseRunValidationResult
        {
            public ICourseRun Run { get; set; }
            public IEnumerable<string> Issues { get; set; }
        }
    }
}
