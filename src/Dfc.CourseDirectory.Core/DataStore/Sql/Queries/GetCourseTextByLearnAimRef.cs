using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseTextByLearnAimRef : ISqlQuery<CourseText>
    {
        public string LearnAimRef { get; set; }
    }
}
