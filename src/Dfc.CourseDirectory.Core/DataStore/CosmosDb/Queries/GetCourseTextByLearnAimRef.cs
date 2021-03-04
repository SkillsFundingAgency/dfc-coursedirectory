using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetCourseTextByLearnAimRef : ICosmosDbQuery<CourseText>
    {
        public string LearnAimRef { get; set; }
    }
}
