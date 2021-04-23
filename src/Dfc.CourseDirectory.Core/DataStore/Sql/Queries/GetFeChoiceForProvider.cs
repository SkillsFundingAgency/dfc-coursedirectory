using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetFeChoiceForProvider : ISqlQuery<FeChoice>
    {
        public int ProviderUkprn { get; set; }
    }
}
