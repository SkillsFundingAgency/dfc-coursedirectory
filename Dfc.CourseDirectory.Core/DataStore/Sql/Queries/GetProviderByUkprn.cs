using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderByUkprn : ISqlQuery<Provider>
    {
        public int Ukprn { get; set; }
    }
}
