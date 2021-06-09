using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    // Given a list of LearningAimRefs return a collection of those that are valid
    public class GetLearningAimRefs : ISqlQuery<IReadOnlyCollection<string>>
    {
        public IEnumerable<string> LearningAimRefs { get; set; }
    }
}
