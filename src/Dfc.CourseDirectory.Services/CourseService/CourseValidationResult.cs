
using System.Linq;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Common;


namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseValidationResult : ValueObject<CourseValidationResult> //, ICourseValidationResult
    {
        public ICourse Course{ get; set; }
        public IList<string> Issues { get; set; }
        public IList<CourseRunValidationResult> RunValidationResults { get; set; }
        public int TotalIssueCount { get { return Issues.Count() + RunValidationResults.SelectMany(c => c.Issues).Count(); } }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Course;
        }

        public class CourseRunValidationResult : ValueObject<CourseRunValidationResult> //, ICourseRunValidationResult
        {
            public ICourseRun Run { get; set; }
            public IEnumerable<string> Issues { get; set; }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Run;
            }
        }
    }

}
