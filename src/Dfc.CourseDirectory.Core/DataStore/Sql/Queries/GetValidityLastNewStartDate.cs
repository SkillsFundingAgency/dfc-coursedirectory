using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetValidityLastNewStartDate : ISqlQuery<List<string>>
    {
        public string LearnAimRef { get; set; }
    }
}
