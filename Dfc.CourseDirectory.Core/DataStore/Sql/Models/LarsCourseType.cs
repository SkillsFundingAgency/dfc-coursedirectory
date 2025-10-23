using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class LarsCourseType
    {
        public string LearnAimRef { get; set; }
        public string CategoryRef { get; set; }
        public CourseType? CourseType { get; set;  }
        public string LearnAimRefTitle { get; set; }
    }
}
